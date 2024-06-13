using SoruxBot.SDK.Model.Message;

namespace SoruxBot.SDK.Plugins.Service;

public static class MessageContextHelper
{
    public static MessageContext WithCurrentContext(this MessageContext messageContext) => messageContext.Clone();
    
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
                messageContext.MessageChain?.DeapClone(),
                messageContext.MessageTime
            );
    }
    
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
            messageContext.MessageChain?.DeapClone(),
            messageContext.MessageTime
        );
    }
    
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
            messageContext.MessageChain?.DeapClone(),
            messageContext.MessageTime
        );
    }
    
    public static bool IsBotAccountMessage(this MessageContext messageContext)
    {
        return messageContext.TriggerId == messageContext.BotAccount;
    }
}