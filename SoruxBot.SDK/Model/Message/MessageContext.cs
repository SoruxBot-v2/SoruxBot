using System;
using System.Collections.Generic;

namespace SoruxBot.SDK.Model.Message;

/// <summary>
/// MessageContext 类 --- 负责传递对应的消息细节
/// </summary>
public class MessageContext(
    string botAccount,
    string targetPlatformAction,
    string targetPlatform,
    MessageType messageEventType,
    string triggerId,
    string triggerPlatformId,
    string tiedId,
    MessageChain? messageChain,
    DateTime messageTime)
{
    public string ContextId { get; init; } = Guid.NewGuid().ToString();
    /// <summary>
    /// 触发该消息的机器人账号
    /// </summary>
    public string BotAccount { get; init; } = botAccount;
    
    /// <summary>
    /// 触发事件的平台动作
    /// </summary>
    public string TargetPlatformAction { get; init; } = targetPlatformAction;
    
    /// <summary>
    /// 触发事件的平台
    /// </summary>
    public string TargetPlatform { get; init; } = targetPlatform;

    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType MessageEventType { get; set; } = messageEventType;

    /// <summary>
    /// 消息触发 ID
    /// </summary>
    public string TriggerId { get; set; } = triggerId;

    /// <summary>
    /// 来源组信息：
    /// 如果是个体信息，那么这个值等于 TriggerId;
    /// 如果是群聊信息，那么这个值等于群组的账号;
    /// 如果是频道信息，那么这个值等于频道的主账号;
    /// </summary>
    public string TriggerPlatformId { get; set; } = triggerPlatformId;

    /// <summary>
    /// 预留的辅助 Id，具体语境根据 TriggerPlatformId 有具体的定义，例如其为频道的主账号时，那么 TiedId 表示的时频道的子频道账号
    /// </summary>
    public string TiedId { get; set; } = tiedId;

    /// <summary>
    /// 消息实体，使用本对象应该遵循语义为主的处理方式
    /// </summary>
    public MessageChain? MessageChain { get; set; } = messageChain;

    /// <summary>
    /// 原始命令参数列表，本列表会存储任何原始的参数信息（不含有母命名头）。
    /// 请仅在无法通过参数注入的情况下使用本命令。
    /// 参数的 Key 为特性注入时提供的参数 Key
    /// </summary>
    public Dictionary<string, object?> CommandParas { get; set; } = new ();

    /// <summary>
    /// 表示消息携带的针对于平台的属性
    /// </summary>
    public Dictionary<string, string> UnderProperty { get; set; } = new ();

    /// <summary>
    /// 表示消息产生的时间戳
    /// </summary>
    public DateTime MessageTime { get; init; } = messageTime;
}