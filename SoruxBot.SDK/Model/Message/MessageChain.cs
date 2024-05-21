using SoruxBot.SDK.Model.Message.Entity;

namespace SoruxBot.SDK.Model.Message;

public class MessageChain(string selfId, string targetId, string platformId, string? tiedId, string platformType)
{
    /// <summary>
    /// 消息链
    /// </summary>
    public List<CommonMessage> Messages { get; set; } = new ();

    /// <summary>
    /// 来源 ID
    /// </summary>
    public string SelfId { get; init; } = selfId;

    /// <summary>
    /// 发送对象 ID
    /// </summary>
    public string TargetId { get; init; } = targetId;

    /// <summary>
    /// 发送平台
    /// </summary>
    public string? PlatformId { get; init; } = platformId;

    /// <summary>
    /// 如果发送平台是多级频道，那么这个表示子频道 ID
    /// </summary>
    public string? TiedId { get; init; } = tiedId;

    /// <summary>
    /// 预先定义的平台类型
    /// </summary>
    public string PlatformType { get; init; } = platformType;
}