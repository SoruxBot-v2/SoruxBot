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
using SoruxBot.Kernel.Constant;
using Org.BouncyCastle.Utilities;
using SoruxBot.Kernel.Services.LogService;
using System.IO;

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
	private ConcurrentRadixTree<string, ConcurrentRadixTree<string, List<PluginsActionDescriptor>>> _prefixRouterTree = new();

	public IServiceCollection Services { get; } = new ServiceCollection();

	public ServiceProvider? ServiceProvider { get; set; }

	/// <summary>
	/// 注册指令路由
	/// </summary>
	/// <param name="filepath"></param>
	/// <param name="name"></param>
	public void RegisterCommandRoute(string filepath, string name)
	{
		var assembly = Assembly.LoadFrom(filepath);
		var types = assembly.GetExportedTypes().ToList();

		var tempRouterTree = _prefixRouterTree.Clone();

		try
		{
			// 遍历类
			var controllerList = types.Where(classType => classType.BaseType == typeof(PluginController)).ToList();
			controllerList.ForEach(classType =>
			{
				RegisterPluginInstance(classType, name)
				.GetMethods().ToList()
				.ForEach(methodInfo =>
				{
					var pluginActiondescriptor = GetActionDescriptor(methodInfo, classType, name);
					RegisterRoute(pluginActiondescriptor);
				});
			});
		}
		catch (Exception ex)
		{
			loggerService.Error(NameValue.KernelPluginServiceDispatcherLogName,
				$"Cannot load plugin {name} due to {ex.Message}");
			return;
		}

		_prefixRouterTree = tempRouterTree;
	}

	/// <summary>
	/// 得到路由被注册后的委托方法
	/// </summary>
	/// <returns></returns>
	public List<PluginsActionDescriptor>? GetAction(ref MessageContext messageContext)
	{
		using var activity = OpenTelemetryHelper.ActivitySource.StartActivity();

		// 监听器匹配
		if (!pluginsListener.Filter(messageContext))
		{
			return null;
		}

		var list = new List<PluginsActionDescriptor>();
		var textRoute = new List<string>();
		var msg = messageContext.MessageChain!.Messages.FirstOrDefault();
		if (msg is not null && msg.Type == "text")
		{
			var textMsg = msg as TextMessage;
			if (textMsg is null)
			{
				loggerService.Warn("PluginsDispatcher", "Message is not TextMessage with payload: " + messageContext);
				return null;
			}
			textRoute.Add(textMsg.Content.Split([' ', '\n', '\t', '\r'])[0]);
		}
		// 路由路径生成
		var route = new List<string>() { messageContext.MessageEventType.ToString() };
		if (messageContext.TargetPlatform != string.Empty)
		{
			route.Add(messageContext.TargetPlatform);
			if (messageContext.TargetPlatformAction != string.Empty)
			{
				route.Add(messageContext.TargetPlatformAction);
			}
		}
		// 消息路由
		// 路由匹配
		var prefixLists = _prefixRouterTree.PrefixMatch(route);
		if (prefixLists == null) return null;
		foreach (var textRouteLists in prefixLists)
		{
			var lists = textRouteLists.Value!.PrefixMatch(textRoute);
			if (lists == null) continue;
			foreach (var l in lists)
			{
				list.AddRange(l.Value!);
			}
		}
		return list.Count > 0 ? list : null;
	}

	private Type RegisterPluginInstance(Type classType, string pluginName)
	{
		loggerService.Debug(NameValue.KernelPluginServiceDispatcherLogName,
			"Controller is caught! Type ->" + classType.Name);
		// 我们只取第一个 Constructor
		var constructorInfo = classType.GetConstructors()[0];
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
			var parameterType = Services.ToList().FirstOrDefault(x =>
			{
				if (x.ServiceType == null) return false;
				if (x.ServiceType.FullName == parameterInfo.ParameterType.FullName) return true;
				return false;
			}
			)!.ServiceType;
			objects.Add(ServiceProvider!.GetRequiredService(parameterType));
		}

		// 构造插件，并存储插件 Controller 实例
		pluginsStorage.SetPluginInstance(pluginName + "." + classType.Name,
		Activator.CreateInstance(classType, objects.ToArray())!);

		return classType;
	}

	private PluginsActionDescriptor? GetActionDescriptor(MethodInfo methodInfo, Type classType, string pluginName)
	{
		var msgEventAttribute = methodInfo.GetCustomAttribute<MessageEventAttribute>();

		if (msgEventAttribute is null) return null;

		// msg command
		var msgEventCommand = methodInfo.GetCustomAttribute<CommandAttribute>();

		PluginsActionDescriptor pluginsActionDescriptor;

		//生成 Controller 的委托
		ParameterInfo[] parameters = methodInfo.GetParameters();

		if (msgEventCommand is null || methodInfo.GetParameters().Length == 1)
		{
			var args = new List<Type>(methodInfo.GetParameters().Select(sp => sp.ParameterType))
						{ methodInfo.ReturnType };
			var delegateType = Expression.GetFuncType(args.ToArray());

			// 如果不存在 Command 那么就不参与 Lexer
			pluginsActionDescriptor = new(
				methodInfo.CreateDelegate(
					delegateType,
					pluginsStorage.GetPluginInstance(pluginName + "." + classType.Name)
				),
				methodInfo.Name,
				pluginName,
				pluginName + "." + classType.Name
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
					pluginsStorage.GetPluginInstance(pluginName + "." + classType.Name)
				),
				methodInfo.Name,
				pluginName,
				pluginName + "." + classType.Name
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
		return pluginsActionDescriptor;
	}

	private void RegisterRoute(PluginsActionDescriptor? pluginsActionDescriptor)
	{
		if (pluginsActionDescriptor == null) return;
		var methodInfo = pluginsActionDescriptor.ActionDelegate.GetMethodInfo();
		var msgEventAttribute = methodInfo.GetCustomAttribute<MessageEventAttribute>()!;
		var msgEventCommand = methodInfo.GetCustomAttribute<CommandAttribute>();

		// 触发消息类型
		string commandTriggerType = msgEventAttribute.MessageType.ToString();

		//判断是否持有平台特定的特性
		var msgPlatformConstraint = methodInfo.GetCustomAttribute<PlatformConstraintAttribute>();

		//消息前缀
		string commandPrefix = msgEventCommand?.CommandPrefix switch
		{
			CommandPrefixType.None => String.Empty,
			CommandPrefixType.Single => pluginsStorage.GetPluginInformation(
				pluginsActionDescriptor.PluginName,
				"CommandPrefixContent"
			),
			CommandPrefixType.Global => _globalCommandPrefix,
			_ => String.Empty
		} ?? String.Empty;
		
		// 路由注册
		var routePrefix = new List<string> { commandTriggerType };
		if (msgPlatformConstraint != null)
		{
			routePrefix.Add(msgPlatformConstraint.Platform);
			if (msgPlatformConstraint.Action != string.Empty)
			{
				routePrefix.Add(msgPlatformConstraint.Action);
			}
		}

		// 由于该方法没有并发调用，故不需要对取出的节点加锁
		if (commandPrefix == string.Empty && (msgEventCommand?.Command.Length ?? 0) == 0)
		{
			_prefixRouterTree.TryInsert(routePrefix, new());
			var textRouter = _prefixRouterTree.GetValue(routePrefix)!;
			RegisterIntoRouterTree(pluginsActionDescriptor, textRouter.Value!, Enumerable.Empty<string>());
		}
		else
		{
			_prefixRouterTree.TryInsert(routePrefix, new());
			var textRouter = _prefixRouterTree.GetValue(routePrefix)!;
			foreach (var s in msgEventCommand!.Command)
			{
				var path = new string[] { commandPrefix + s.Split([' ', '\n', '\t', '\r'])[0] };
				RegisterIntoRouterTree(pluginsActionDescriptor, textRouter.Value!, path);
			}
		}
	}

	private void RegisterIntoRouterTree(PluginsActionDescriptor pluginsActionDescriptor, 
		ConcurrentRadixTree<string, List<PluginsActionDescriptor>> textRouter,
		IEnumerable<string> path)
	{
		if (textRouter.TryGetValue(path, out var list))
		{
			list!.Value!.Add(pluginsActionDescriptor);
		}
		else
		{
			var newList = new List<PluginsActionDescriptor>() { pluginsActionDescriptor };
			textRouter.Insert(path, newList);
		}
	}
}