using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoruxBot.Kernel.Bot;
using SoruxBot.Kernel.Interface;
using SoruxBot.Kernel.Services.PluginService.Model;
using SoruxBot.Kernel.Services.PluginService.DataStructure;
using SoruxBot.SDK.Attribute;
using SoruxBot.SDK.Model.Attribute;
using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Plugins.Basic;
using SoruxBot.SDK.Plugins.Service;
using System.Text;
using SoruxBot.SDK.Model.Message.Entity;
using System.Collections.Generic;

namespace SoruxBot.Kernel.Services.PluginService;

/// <summary>
/// 插件调度器，负责分配事件到具体的插件。
/// </summary>
/// <param name="botContext"></param>
/// <param name="loggerService"></param>
/// <param name="pluginsStorage"></param>
/// <param name="configuration"></param>
/// <param name="pluginsListener"></param>
public class PluginsDispatcher(
    BotContext botContext,
    ILoggerService loggerService,
    IPluginsStorage pluginsStorage,
    IConfiguration configuration,
    PluginsListener pluginsListener)
{
    private readonly string _globalCommandPrefix = configuration.GetSection("chat:global_prefix").Value ?? "";

    //插件按照触发条件可以分为选项式命令触发和事件触发
    //前者针对某个特定 EventType 的某个特定的语句触发某个特定的方法
    //后者针对某个通用的 EventType 进行触发
    private readonly RadixTree<RadixTree<List<PluginsActionDescriptor>>> _prefixRouterTree = new();

    public IServiceCollection Services { get; } = new ServiceCollection();

    public ServiceProvider? ServiceProvider { get; set; }
    
    /// <summary>
    /// 注册指令路由
    /// </summary>
    /// <param name="filepath"></param>
    /// <param name="name"></param>
    public void RegisterCommandRoute(string filepath, string name)
    {
        var assembly = Assembly.LoadFile(filepath);
        var types = assembly.GetExportedTypes();

        // 遍历类
        foreach (var className in types)
        {
            if (className.BaseType == typeof(PluginController))
            {
                loggerService.Debug(Constant.NameValue.KernelPluginServiceDispatcherLogName,
                    "Controller is caught! Type ->" + className.Name);
                // 我们只取第一个 Constructor
                var constructorInfo = className.GetConstructors()[0];
                // 获取其构造函数的参数
                var parameterInfos = constructorInfo.GetParameters();
                // 构造参数匹配表
                var objects = new List<object>();

                var serviceProvider = botContext.ServiceProvider;
                foreach (var parameterInfo in parameterInfos)
                {
                    if (parameterInfo.ParameterType == typeof(BotContext))
                    {
                        objects.Add(botContext);
                        continue;
                    }
	                
                    if (parameterInfo.ParameterType == typeof(ILoggerService))
                    {
                        objects.Add(loggerService);
                        continue;
                    }
                    
                    var ctxObj = serviceProvider.GetService(parameterInfo.ParameterType);
                    if (ctxObj is not null) 
                    {
                        objects.Add(ctxObj);
                        continue;
                    }
                    
                    // 如果是 Null，那么一定得能从插件依赖中找到
                    objects.Add(ServiceProvider!.GetRequiredService(parameterInfo.ParameterType));
                }

                // 构造插件，并存储插件 Controller 实例
                pluginsStorage.SetPluginInstance(name + "." + className.Name,
                    Activator.CreateInstance(className, objects.ToArray())!);

                var methods = className.GetMethods();
                foreach (var methodInfo in methods)
                {
                    var msgEventAttribute = methodInfo.GetCustomAttribute<MessageEventAttribute>();

                    if (msgEventAttribute is null)
                    {
                        continue;
                    }

                    // msg command
                    var msgEventCommand = methodInfo.GetCustomAttribute<CommandAttribute>();

                    PluginsActionDescriptor pluginsActionDescriptor;

                    //生成 Controller 的委托
                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    if (msgEventCommand is null)
                    {
                        var args = new List<Type>(methodInfo.GetParameters().Select(sp => sp.ParameterType))
                            { methodInfo.ReturnType };
                        var delegateType = Expression.GetFuncType(args.ToArray());

                        // 如果不存在 Command 那么就不参与 Lexer
                        pluginsActionDescriptor = new(
                            methodInfo.CreateDelegate(
                                delegateType,
                                pluginsStorage.GetPluginInstance(name + "." + className.Name)
                            ),
                            methodInfo.Name,
                            name,
                            name + "." + className.Name
                        );
                    }
                    else if (methodInfo.GetParameters().Length == 1) // 如果参数为 1，也就是只接受一个 MessageContext 参数，那么就不需要 Lexer
                    {
                        var args = new List<Type>(methodInfo.GetParameters().Select(sp => sp.ParameterType))
                            { methodInfo.ReturnType };
                        var delegateType = Expression.GetFuncType(args.ToArray());

                        // 如果不需要的话，那么就直接将参数传递给第一个参数，并且不进行 Lexer 绑定
                        pluginsActionDescriptor = new(
                            methodInfo.CreateDelegate(
                                delegateType,
                                pluginsStorage.GetPluginInstance(name + "." + className.Name)
                            ),
                            methodInfo.Name,
                            name,
                            name + "." + className.Name
                        );
                    }
                    else
                    {
                        // 因为我们认为的多命令对应一个 Action 指的是触发条件可以多个
                        // 而触发条件实际上就是 Command 第一个头不一样
                        // 因此 Para 只需要计算一遍即可

                        string[] paras = msgEventCommand.Command[0].Split(" ").Skip(1).ToArray();
                        int count = 0;

                        // 构建委托
                        var args = new List<Type>(methodInfo.GetParameters().Select(sp => sp.ParameterType))
                            { methodInfo.ReturnType };
                        var delegateType = Expression.GetFuncType(args.ToArray());

                        pluginsActionDescriptor = new(
                            methodInfo.CreateDelegate(
                                delegateType,
                                pluginsStorage.GetPluginInstance(name + "." + className.Name)
                            ),
                            methodInfo.Name,
                            name,
                            name + "." + className.Name
                        );

                        //添加必然存在的参数 MessageContext
                        var messageContextPara = new PluginActionParameter(
                            false,
                            typeof(MessageContext),
                            "context"
                        );

                        pluginsActionDescriptor.ActionParameters.Add(messageContextPara);

                        foreach (var parameterInfo in parameters.Skip(1))
                        {
                            //默认插件作者提供的命令列表的参数顺序和 Action 的函数顺序一致，否者绑定失败需要作者自己从 Context 获取

                            // [var] 表示必选参数， <var> 表示可选参数
                            var pluginsActionParameter = new PluginActionParameter(
                                paras[count].Substring(0, 1).Equals("["),
                                parameterInfo.ParameterType,
                                paras[count].Substring(1, paras[count].Length - 2)
                            );

                            pluginsActionDescriptor.ActionParameters.Add(pluginsActionParameter);
                            count++;
                        }
                    }

                    // 触发消息类型
                    string commandTriggerType = msgEventAttribute.MessageType.ToString();

                    //判断是否持有平台特定的特性
                    var msgPlatformConstraint = methodInfo.GetCustomAttribute<PlatformConstraintAttribute>();

                    //消息前缀
                    string commandPrefix = msgEventCommand?.CommandPrefix switch
                    {
                        CommandPrefixType.None => String.Empty,
                        CommandPrefixType.Single => pluginsStorage.GetPluginInformation(
                            name,
                            "CommandPrefixContent"
                        ),
                        CommandPrefixType.Global => _globalCommandPrefix,
                        _ => String.Empty
                    } ?? String.Empty;

					// 路由注册
					var routePrefix = new StringBuilder(commandTriggerType);
					if (msgPlatformConstraint != null)
					{
						routePrefix.Append(";" + msgPlatformConstraint.Platform);
						if(msgPlatformConstraint.Action != string.Empty)
						{
							routePrefix.Append(";" + msgPlatformConstraint.Action);
						}
					}
					
					if(commandPrefix == string.Empty && (msgEventCommand?.Command.Length ?? 0) == 0)
					{
						_prefixRouterTree.TryInsert(routePrefix.ToString(), new RadixTree<List<PluginsActionDescriptor>>());
						var textRouter = _prefixRouterTree.GetValue(routePrefix.ToString())!;
						if (textRouter.TryGetValue(string.Empty, out var list))
						{
							list!.Add(pluginsActionDescriptor);
						}
						else
						{
							list = new List<PluginsActionDescriptor>() { pluginsActionDescriptor };
							textRouter.Insert(string.Empty, list);
						}
						
					}
					else
					{
						_prefixRouterTree.TryInsert(routePrefix.ToString(), new RadixTree<List<PluginsActionDescriptor>>());
						var textRouter = _prefixRouterTree.GetValue(routePrefix.ToString())!;
						foreach (var s in msgEventCommand!.Command)
						{
							string path = new StringBuilder().Append('/').Append(commandPrefix).Append(s.Split(" ")[0]).ToString();
							if (textRouter.TryGetValue(path, out var list))
							{
								list!.Add(pluginsActionDescriptor);
							}
							else
							{
								list = new List<PluginsActionDescriptor>() { pluginsActionDescriptor };
								textRouter.Insert(path, list);
							}
						}
					}
				}
            }
        }
    }

    /// <summary>
    /// 得到路由被注册后的委托方法
    /// </summary>
    /// <returns></returns>
    public List<PluginsActionDescriptor>? GetAction(ref MessageContext messageContext)
    {
	    // 监听器匹配
	    if (!pluginsListener.Filter(messageContext))
	    {
		    return null;
	    }
		var list = new List<PluginsActionDescriptor>();
		var msg = messageContext.MessageChain!.Messages[0];
		string textRoute = string.Empty;
		if (msg.Type == "text")
		{
			var textMsg = (TextMessage)msg;
			textRoute = "/" + textMsg.Content.Split(' ')[0];
		}
		// 路由路径生成
	    StringBuilder route = new StringBuilder(messageContext.MessageEventType.ToString());
		if(messageContext.TargetPlatform !=string.Empty)
		{
			route.Append(';').Append(messageContext.TargetPlatform);
			if(messageContext.TargetPlatformAction != string.Empty)
			{
				route.Append(';').Append(messageContext.TargetPlatformAction);
			}
		}
		// 消息路由
		// 路由匹配
		var prefixLists = _prefixRouterTree.PrefixMatch(route.ToString());
		if (prefixLists == null) return null;
		foreach (var textRouteLists in prefixLists)
		{
			var lists = textRouteLists.PrefixMatch(textRoute);
			if (lists == null) continue;
			foreach(var l in lists)
			{
				list.AddRange(l);
			}
		}
		return list.Count > 0 ? list : null;
	}
}