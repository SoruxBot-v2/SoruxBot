using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Plugins.Service;

namespace SoruxBot.Kernel.Interface;

public interface IResponseQueue
{
    public Task<MessageResult> SetNextResponseAsync(MessageContext context);

    public IResponsePromise SetNextResponse(MessageContext context);

    public bool TryGetNextResponse(Func<MessageContext, MessageResult> func);
}