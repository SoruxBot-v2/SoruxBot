namespace SoruxBot.SDK.Model.Message.Entity;

/// <summary>
/// 通用消息实体
/// </summary>
public class CommonMessage(string type, Dictionary<string, object> content)
{
    /// <summary>
    /// 消息类型
    /// 约束：text 类型为纯文字消息
    /// </summary>
    public string Type { get; } = type;

    /// <summary>
    /// 消息内容
    /// 使用 Dictionary 来存储消息内容。
    /// 约束：content 属性用于存储文字内容
    /// </summary>
    public Dictionary<string, object> Content { get; } = content;
    
    /// <summary>
    /// 预览消息
    /// </summary>
    /// <returns></returns>
    public virtual string ToPreviewText() => "[暂不支持该消息类型]";
}