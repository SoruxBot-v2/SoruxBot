using SoruxBot.SDK.Model.Message;

namespace SoruxBot.SDK.Plugins.Service;

public interface IResponsePromise
{
    public void Then(Action<MessageResult> messageCallback);
}