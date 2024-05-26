namespace SoruxBot.SDK.Model.Utils;

public class ResponsePromise
{
    // 私有的委托列表
    public List<Action<string>> Callbacks { get; } = [];

    public void Then(Action<string> messageCallback)
    {
        Callbacks.Add(messageCallback);
    }
}