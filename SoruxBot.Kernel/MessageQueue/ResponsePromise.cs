using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Plugins.Service;

namespace SoruxBot.Kernel.MessageQueue;

public class ResponsePromise : IResponsePromise
{
    // 私有的委托列表
    public List<Action<MessageResult>> Callbacks { get; } = [];

    public void Then(Action<MessageResult> messageCallback)
    {
        Callbacks.Add(messageCallback);
    }
}