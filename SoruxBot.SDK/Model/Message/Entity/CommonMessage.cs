namespace SoruxBot.SDK.Model.Message.Entity;

/// <summary>
/// 通用消息实体
/// </summary>
public class CommonMessage(string type)
{
    /// <summary>
    /// 消息类型
    /// 约束：text 类型为纯文字消息
    /// </summary>
    public string Type { get; } = type;
    
    /// <summary>
    /// 预览消息
    /// </summary>
    /// <returns></returns>
    public virtual string ToPreviewText() => "[暂不支持该消息类型]";
    
    /// <summary>
    /// 克隆对象
    /// </summary>
    /// <returns></returns>
    public virtual CommonMessage DeapClone()
    {
        return new CommonMessage(type);
    }
}