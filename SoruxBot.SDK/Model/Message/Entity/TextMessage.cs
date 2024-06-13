namespace SoruxBot.SDK.Model.Message.Entity;

public class TextMessage(string content) : CommonMessage("text")
{
    /// <summary>
    /// 文字消息的消息内容
    /// </summary>
    public string Content { get; init; } = content;

    public override string ToPreviewText() => Content;
    
    /// <summary>
    /// 克隆对象
    /// </summary>
    /// <returns></returns>
    public override CommonMessage DeapClone()
    {
        return new TextMessage(Content);
    }
}