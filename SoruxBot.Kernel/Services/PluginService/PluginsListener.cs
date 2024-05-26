using SoruxBot.Kernel.Bot;
using SoruxBot.Kernel.Services.PluginService.Model;
using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Plugins.Service;

namespace SoruxBot.Kernel.Services.PluginService;

// TODO 构建一颗匹配树
public class PluginsListener(BotContext botContext, ILoggerService loggerService)
{
    private BotContext _botContext = botContext;
    private ILoggerService _loggerService = loggerService;

    private readonly List<PluginsListenerDescriptor> _map = new ();

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
    public bool Filter(MessageContext context)
    {
        foreach (var descriptor in _map)
        {
            if (MatchRoute(descriptor, context) && descriptor.ConditionCheck(context))
            {
                // TODO 这里应该是构建一个匹配树，然后 Foreach，而不是 Foreach 的时候来判断是否被匹配
            }
        }

        return true;
    }

    public void RemoveListener(PluginsListenerDescriptor pluginsListenerDescriptor)
        => _map.Remove(pluginsListenerDescriptor);

    public void AddListener(PluginsListenerDescriptor pluginsListenerDescriptor)
        => _map.Add(pluginsListenerDescriptor);
}