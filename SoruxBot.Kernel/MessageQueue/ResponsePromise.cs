using SoruxBot.Kernel.Interface;

namespace SoruxBot.Kernel.MessageQueue;

public class ResponsePromise : IResponsePromise
{
    // 私有的委托列表
    public List<Action<string>> Callbacks { get; } = [];

    public void Then(Action<string> messageCallback)
    {
        Callbacks.Add(messageCallback);
    }
}