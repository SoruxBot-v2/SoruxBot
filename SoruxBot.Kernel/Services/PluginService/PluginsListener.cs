using SoruxBot.Kernel.Bot;
using SoruxBot.Kernel.Services.PluginService.DataStructure;
using SoruxBot.Kernel.Services.PluginService.Model;
using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Plugins.Service;
using System.Text;
using SoruxBot.SDK.Plugins.Model;

namespace SoruxBot.Kernel.Services.PluginService;

public class PluginsListener(BotContext botContext, ILoggerService loggerService)
{
    private BotContext _botContext = botContext;
    private ILoggerService _loggerService = loggerService;

	private readonly RadixTree<PluginsListenerDescriptor> _matchTree = new();

    /// <summary>
    /// 进入Filter队列，并且判断是否需要继续执行 Dispatcher
    /// 如果返回 false 那么说明不需要继续进入 Message 路由
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public bool Filter(MessageContext context)
    {
		StringBuilder path = new StringBuilder(context.MessageEventType.ToString());
		if (context.TargetPlatform != string.Empty)
		{
			path = path.Append(";").Append(context.TargetPlatform);
			if (context.TargetPlatformAction != string.Empty)
			{
				path = path.Append(";").Append(context.TargetPlatformAction);
			}
		}
		// TODO 并发处理
		var list = _matchTree.PrefixMatch(path.ToString());
		if (list == null) return true;
		foreach (var item in list)
		{
			if(item.ConditionCheck(context))
			{
				item.SuccessfulFunc(context);
			}
			if(item.IsInterceptToFilters)
			{
				break;
			}
			
		}
		if(list.Any(item => item.IsInterceptToChannel))
		{
			return false;
		}
		return true;
    }

    public void RemoveListener(PluginsListenerDescriptor pluginsListenerDescriptor)
	{
		StringBuilder path = new StringBuilder(pluginsListenerDescriptor.MessageType.ToString());
		if(pluginsListenerDescriptor.TargetPlatformType != string.Empty)
		{
			path = path.Append(";").Append(pluginsListenerDescriptor.TargetPlatformType);
			if(pluginsListenerDescriptor.TargetAction !=  string.Empty)
			{
				path = path.Append(";").Append(pluginsListenerDescriptor.TargetAction);
			}
		}
		// TODO 并发处理
		_matchTree.Remove(path.ToString());
	}

    public void AddListener(PluginsListenerDescriptor pluginsListenerDescriptor)
	{
		StringBuilder path = new StringBuilder(pluginsListenerDescriptor.MessageType.ToString());
		if (pluginsListenerDescriptor.TargetPlatformType != string.Empty)
		{
			path = path.Append(";").Append(pluginsListenerDescriptor.TargetPlatformType);
			if (pluginsListenerDescriptor.TargetAction != string.Empty)
			{
				path = path.Append(";").Append(pluginsListenerDescriptor.TargetAction);
			}
		}
		// TODO 并发处理
		if (_matchTree.ContainsPath(path.ToString()))
		{
			_matchTree.TryReplace(path.ToString(), pluginsListenerDescriptor);
		}
		else
		{
			_matchTree.Insert(path.ToString(), pluginsListenerDescriptor);
		}
	}
}