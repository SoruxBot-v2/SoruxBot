using SoruxBot.SDK.Model.Message;

namespace SoruxBot.SDK.Plugins.Model;

public class PluginsListenerDescriptor(
    MessageType messageType,
    string targetPlatformType,
    string targetAction,
    Func<MessageContext, bool> conditionCheck,
	bool isInterceptToFilters = true,
    bool isInterceptToChannel = true)
{
	/// <summary>
	/// 标识唯一标识符
	/// </summary>
	public string ID { get; init; } = Guid.NewGuid().ToString();
    /// <summary>
    /// 监听事件的类型
    /// </summary>
    public MessageType MessageType { get; init; } = messageType;

    /// <summary>
    /// 监听事件的平台
    /// </summary>
    public string TargetPlatformType { get; init; } = targetPlatformType;

    /// <summary>
    /// 监听事件的方法
    /// </summary>
    public string TargetAction { get; init; } = targetAction;

    /// <summary>
    /// 监听事件的条件
    /// </summary>
    public Func<MessageContext, bool> ConditionCheck { get; init; } = conditionCheck;

    /// <summary>
    /// 是否禁止消息继续传递，表示禁止其他监听器继续监听消息
    /// </summary>
    public bool IsInterceptToFilters { get; init; } = isInterceptToFilters;
    
    /// <summary>
    /// 是否禁止消息继续传递，表示禁止让捕获到到 Context 进入到消息路由管道中
    /// </summary>
    public bool IsInterceptToChannel { get; init; } = isInterceptToChannel;
}