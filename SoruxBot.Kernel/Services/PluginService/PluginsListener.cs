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

	private readonly ConcurrentRadixTree<string, PluginsListenerDescriptor> _matchTree = new();

    /// <summary>
    /// 进入Filter队列，并且判断是否需要继续执行 Dispatcher
    /// 如果返回 false 那么说明不需要继续进入 Message 路由
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public bool Filter(MessageContext context)
    {
		var path = new List<string>() { context.MessageEventType.ToString() };
		if (context.TargetPlatform != string.Empty)
		{
			path.Add(context.TargetPlatform);
			if (context.TargetPlatformAction != string.Empty)
			{
				path.Add(context.TargetPlatformAction);
			}
		}
		var list = _matchTree.PrefixMatch(path);
		if (list.Length == 0) return true;

		bool isInterceptedToChannel = false;
		foreach (var item in list)
		{
			if (item.ConditionCheck(context))
			{
				item.SuccessfulFunc(context);
				if (item.IsInterceptToChannel) isInterceptedToChannel = true;
				if (item.IsInterceptToFilters) break;
			}
			
		}
		return !isInterceptedToChannel;
    }

    public void RemoveListener(PluginsListenerDescriptor pluginsListenerDescriptor)
	{
		var path = new List<string>() { pluginsListenerDescriptor.MessageType.ToString() };
		if(pluginsListenerDescriptor.TargetPlatformType != string.Empty)
		{
			path.Add(pluginsListenerDescriptor.TargetPlatformType);
			if(pluginsListenerDescriptor.TargetAction !=  string.Empty)
			{
				path.Add(pluginsListenerDescriptor.TargetAction);
			}
		}
		_matchTree.Remove(path);
	}

    public void AddListener(PluginsListenerDescriptor pluginsListenerDescriptor)
	{
		var path = new List<string>() { pluginsListenerDescriptor.MessageType.ToString() };
		if (pluginsListenerDescriptor.TargetPlatformType != string.Empty)
		{
			path.Add(pluginsListenerDescriptor.TargetPlatformType);
			if (pluginsListenerDescriptor.TargetAction != string.Empty)
			{
				path.Add(pluginsListenerDescriptor.TargetAction);
			}
		}
		_matchTree.ForceInsert(path, pluginsListenerDescriptor);
	}
}