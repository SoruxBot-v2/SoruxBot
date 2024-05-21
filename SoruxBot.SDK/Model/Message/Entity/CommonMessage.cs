namespace SoruxBot.SDK.Model.Message.Entity;

/// <summary>
/// 通用消息实体
/// </summary>
public class CommonMessage(string type, string content)
{
    /// <summary>
    /// 消息类型
    /// text, picture, mention
    /// </summary>
    public string Type { get; init; } = type;

    /// <summary>
    /// 消息内容
    /// text: 文本内容
    /// picture: 图片 URL
    /// mention: 被提及的用户 ID
    /// </summary>
    public string Content { get; init; } = content;
    
    /// <summary>
    /// 预览消息
    /// </summary>
    /// <returns></returns>
    public virtual string ToPreviewText() => "[暂不支持该消息类型]";
}