using SoruxBot.Kernel.Bot;
using SoruxBot.Kernel.Services.PluginService.DataStructure;
using SoruxBot.Kernel.Services.PluginService.Model;
using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Plugins.Service;
using System.Text;

namespace SoruxBot.Kernel.Services.PluginService;

// TODO 构建一颗匹配树
public class PluginsListener(BotContext botContext, ILoggerService loggerService)
{
    private BotContext _botContext = botContext;
    private ILoggerService _loggerService = loggerService;

	private readonly RadixTree<PluginsListenerDescriptor> _matchTree = new();

    private bool MatchRoute(PluginsListenerDescriptor descriptor, MessageContext context)
    {
        return false;
    }

    /// <summary>
    /// 进入Filter队列，并且判断是否需要继续执行 Dispatcher
    /// 如果返回 false 那么说明不需要继续进入 Message 路由
    /// </summary>
    /// <param name="context"></param>
    /// <param name="newContext"></param>
    /// <returns></returns>
    public bool Filter(MessageContext context, out MessageContext newContext)
    {
        newContext = context;
		string path = new StringBuilder(context.MessageEventType.ToString())
			.Append(context.TargetPlatform)
			.ToString();
			
        //_matchTree.PrefixMatch()

        return true;
    }

    public void RemoveListener(PluginsListenerDescriptor pluginsListenerDescriptor)
	{
		string path = new StringBuilder(pluginsListenerDescriptor.MessageType.ToString())
			.Append(";")
			.Append(pluginsListenerDescriptor.TargetPlatformType)
			.Append(";")
			.Append(pluginsListenerDescriptor.TargetAction)
			.ToString();
		_matchTree.Remove(path);
	}

    public void AddListener(PluginsListenerDescriptor pluginsListenerDescriptor)
	{
		string path = new StringBuilder(pluginsListenerDescriptor.MessageType.ToString())
			.Append(";")
			.Append(pluginsListenerDescriptor.TargetPlatformType)
			.Append(";")
			.Append(pluginsListenerDescriptor.TargetAction)
			.ToString();
		if (_matchTree.ContainsPath(path))
		{
			_matchTree.TryReplace(path, pluginsListenerDescriptor);
		}
		else
		{
			_matchTree.Insert(path, pluginsListenerDescriptor);
		}
	}
}