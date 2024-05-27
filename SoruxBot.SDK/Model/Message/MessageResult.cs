namespace SoruxBot.SDK.Model.Message;

public class MessageResult(string id, DateTime timestamp)
{
    /// <summary>
    /// Message ID
    /// </summary>
    public string Id { get; init; } = id;

    /// <summary>
    /// 消息发送出去的时间戳
    /// </summary>
    public DateTime Timestamp { get; init; } = timestamp;

    /// <summary>
    /// 附带的消息属性
    /// </summary>
    public Dictionary<string, string> UnderProperty { get; init; } = new();
}