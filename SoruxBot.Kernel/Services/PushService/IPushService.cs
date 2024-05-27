using SoruxBot.SDK.Model.Message;

namespace SoruxBot.Kernel.Services.PushService;

public interface IPushService
{
    public void RunInstance(Action<MessageContext> messageCallback, Func<MessageContext, MessageResult> responseCallback);
    public void StopInstance();
}