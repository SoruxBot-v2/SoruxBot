using System.Text;
using SoruxBot.Kernel.Constant;
using SoruxBot.Kernel.Interface;
using SoruxBot.Kernel.Services.PluginService.Model;
using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Model.Message.Entity;
using SoruxBot.SDK.Plugins.Model;
using SoruxBot.SDK.Plugins.Service;

namespace SoruxBot.Kernel.Services.PluginService;

/// <summary>
/// 用于解析插件 Action的参数包，将规范委托转换为特定委托实现参数注入
/// </summary>
public class PluginsCommandLexer(ILoggerService loggerService, IPluginsStorage pluginsStorage)
{
    private IPluginsStorage _pluginsStorage = pluginsStorage;

    /// <summary>
    /// 将 Context 传入 Lexer 模块供 Action 使用
    /// </summary>
    /// <param name="context"></param>
    /// <param name="descriptor"></param>
    /// <returns></returns>
    public PluginFlag PluginAction(MessageContext context, PluginsActionDescriptor descriptor)
    {
        // 这个方法中，调用的类型依次可以被解析为
        // object > MessageChain > string > bool > int

        var objects = new List<object?> { context };

        var msgs = context.MessageChain?.Messages;
        
        if (msgs is not null && descriptor.ActionParameters.Count >= 2)
        {
            // 如果参数长度不匹配，那么 pass
            var isValid = true;

            var parasCount = 0;
            var paras =
                msgs.Select(sp => sp.Type switch
                {
                    "text" => sp.ToPreviewText()
                        .Split(" ")
                        .Select(text => new TextMessage(text) as CommonMessage)
                        .ToList(),
                    _      => [sp]
                }).SelectMany(sp => sp).Skip(1).ToList();

            if (descriptor.ActionParameters
                    .Skip(msgs.Count)
                    .Count(sp => !sp.IsOptional) > paras.Count)
            {
                // 表示消息没有被处理
                loggerService.Warn(NameValue.KernelPluginServiceLexerLogName,
                    "Lexer error for unmatched parameter number. "
                    + ", PluginsName:" + descriptor.InstanceTypeName
                    + ", ParameterCount:" + descriptor.ActionParameters.Count
                    + ", MessageCount:" + msgs.Count);
                return PluginFlag.MsgUnprocessed;
            }
            
            descriptor.ActionParameters
                .Skip(1)
                .ToList()
                .ForEach(sp =>
                {
                    if (parasCount > paras.Count - 1)
                    {
                        objects.Add(null);
                        parasCount++;
                        return;
                    }

                    // 开始处理剩余参数
                    if (sp.ParameterType.BaseType == typeof(CommonMessage) || sp.ParameterType == typeof(object))
                    {
                        objects.Add(paras[parasCount]);
                        parasCount++;
                        return;
                    }
                    
                    if (sp.ParameterType == typeof(string))
                    {
						objects.Add(paras[parasCount].ToPreviewText());
                        parasCount++;
                        return;
					}
                    
                    if (sp.ParameterType == typeof(bool))
                    {
                        if (bool.TryParse(paras[parasCount].ToPreviewText(), out bool result))
                        {
                            objects.Add(result);
                        }
                        else
                        {
                            isValid = false;
                        }
                        parasCount++;
                        return;
                    }
                    
                    if (sp.ParameterType == typeof(int))
                    {
                        if (int.TryParse(paras[parasCount].ToPreviewText(), out int result))
                        {
                            objects.Add(result);
                        }
                        else
                        {
                            isValid = false;
                        }
                        parasCount++;
                        return;
                    }
                    
                    parasCount++;
                    isValid = false;
                });


            if (!isValid)
            {
                // 表示消息没有被处理
                loggerService.Warn(NameValue.KernelPluginServiceLexerLogName,
                    "Lexer error for type unmatched during lexer."
                    + ", PluginsName:" + descriptor.InstanceTypeName
                    + ", ActionName:" + descriptor.ActionName);
                return PluginFlag.MsgUnprocessed;
            }
        }

        return (PluginFlag)descriptor.ActionDelegate.DynamicInvoke(objects.ToArray())!;
    }
}