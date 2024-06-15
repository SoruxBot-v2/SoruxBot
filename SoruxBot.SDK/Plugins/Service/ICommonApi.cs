using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Plugins.Model;

namespace SoruxBot.SDK.Plugins.Service;

public interface ICommonApi
{
    /// <summary>
    /// 发送消息，并将消息的返回值作为 Task 对象返回
    /// </summary>
    /// <param name="messageContext"></param>
    /// <returns></returns>
    public Task<MessageResult> SendMessageAsync(MessageContext messageContext);
    /// <summary>
    /// 直接发送消息，返回 ResponsePromise，可以根据需要进行回调
    /// </summary>
    /// <param name="messageContext"></param>
    /// <returns></returns>
    public IResponsePromise SendMessage(MessageContext messageContext);
    /// <summary>
    /// 注册监听器
    /// </summary>
    /// <param name="pluginsListenerDescriptor"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<MessageContext?> RegisterListenerAsync(PluginsListenerDescriptor pluginsListenerDescriptor, CancellationToken cancellationToken = default);
}