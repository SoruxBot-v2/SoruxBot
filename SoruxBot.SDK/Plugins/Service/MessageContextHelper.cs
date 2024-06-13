using SoruxBot.SDK.Model.Message;

namespace SoruxBot.SDK.Plugins.Service;

public static class MessageContextHelper
{
    /// <summary>
    /// 用当前 Context 构造一个新的 Context
    /// </summary>
    /// <param name="messageContext"></param>
    /// <returns></returns>
    public static MessageContext WithCurrentContext(this MessageContext messageContext) => messageContext.DeepClone();
    
    /// <summary>
    /// 用当前 Context 和新的 TriggerId 构造一个新的 Context
    /// </summary>
    /// <param name="messageContext"></param>
    /// <param name="triggerId"></param>
    /// <returns></returns>
    public static MessageContext WithNewTriggerId(this MessageContext messageContext, string triggerId)
    {
        return new MessageContext(
                messageContext.BotAccount,
                messageContext.TargetPlatformAction,
                messageContext.TargetPlatform,
                messageContext.MessageEventType,
                triggerId,
                messageContext.TriggerPlatformId,
                messageContext.TiedId,
                messageContext.MessageChain?.DeepClone(),
                messageContext.MessageTime
            );
    }
    
    /// <summary>
    /// 用户当前 Context 和新的 TriggerPlatformId 构造一个新的 Context
    /// </summary>
    /// <param name="messageContext"></param>
    /// <param name="triggerPlatformId"></param>
    /// <returns></returns>
    public static MessageContext WithNewTriggerPlatformId(this MessageContext messageContext, string triggerPlatformId)
    {
        return new MessageContext(
            messageContext.BotAccount,
            messageContext.TargetPlatformAction,
            messageContext.TargetPlatform,
            messageContext.MessageEventType,
            messageContext.TriggerId,
            triggerPlatformId,
            messageContext.TiedId,
            messageContext.MessageChain?.DeepClone(),
            messageContext.MessageTime
        );
    }
    
    /// <summary>
    /// 用户当前 Context 和新的 MessageChain 构造一个新的 Context
    /// </summary>
    /// <param name="messageContext"></param>
    /// <param name="chain"></param>
    /// <returns></returns>
    public static MessageContext WithNewMessageChain(this MessageContext messageContext, MessageChain chain)
    {
        return new MessageContext(
            messageContext.BotAccount,
            messageContext.TargetPlatformAction,
            messageContext.TargetPlatform,
            messageContext.MessageEventType,
            messageContext.TriggerId,
            messageContext.TriggerPlatformId,
            messageContext.TiedId,
            chain,
            messageContext.MessageTime
        );
    }
    
    /// <summary>
    /// 用户当前 Context 和新的 BotAccount 构造一个新的 Context
    /// e.g. QQ -> WeChat 平台需要转发消息，但是请注意 MessageChain 的类型转化问题
    /// </summary>
    /// <param name="messageContext"></param>
    /// <param name="botAccount"></param>
    /// <returns></returns>
    public static MessageContext WithNewBotAccount(this MessageContext messageContext, string botAccount)
    {
        return new MessageContext(
            botAccount,
            messageContext.TargetPlatformAction,
            messageContext.TargetPlatform,
            messageContext.MessageEventType,
            messageContext.TriggerId,
            messageContext.TriggerPlatformId,
            messageContext.TiedId,
            messageContext.MessageChain?.DeepClone(),
            messageContext.MessageTime
        );
    }
    
    /// <summary>
    /// 判断是否为 Bot 账号消息
    /// </summary>
    /// <param name="messageContext"></param>
    /// <returns></returns>
    public static bool IsBotAccountMessage(this MessageContext messageContext)
    {
        return messageContext.TriggerId == messageContext.BotAccount;
    }
}