using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Plugins.Model;
using SoruxBot.SDK.Plugins.Service;

namespace SoruxBot.SDK.QQ;

public static class QqExternApi
{
    // TODO 完成剩下的 API
    public static Task<MessageResult> QqSendFriendMessageAsync(this ICommonApi api, MessageChain chain, string botAccount)
    {
        var ctx = new MessageContext(
            botAccount,
            "SendFriendMessage",
            Constant.QqPlatformType,
            MessageType.PrivateMessage,
            chain.TargetId,
            chain.TargetId,
            chain.TargetId,
            chain,
            DateTime.Now
            );
        return api.SendMessageAsync(ctx);
    }
    
    public static IResponsePromise QqSendFriendMessage(this ICommonApi api, MessageChain chain, string botAccount)
    {
        var ctx = new MessageContext(
            botAccount,
            "SendFriendMessage",
            Constant.QqPlatformType,
            MessageType.PrivateMessage,
            chain.TargetId,
            chain.TargetId,
            chain.TargetId,
            chain,
            DateTime.Now
        );
        return api.SendMessage(ctx);
    }
    
    public static Task<MessageResult> QqSendGroupMessageAsync(this ICommonApi api, MessageChain chain, string botAccount)
    {
        var ctx = new MessageContext(
            botAccount,
            "SendGroupMessage",
            Constant.QqPlatformType,
            MessageType.GroupMessage,
            chain.TargetId,
            chain.PlatformId ?? throw new ArgumentException("PlatformId is null, however invoke SendGroupMessageAsync"),
            chain.TargetId,
            chain,
            DateTime.Now
        );
        return api.SendMessageAsync(ctx);
    }
    
    public static IResponsePromise QqSendGroupMessage(this ICommonApi api, MessageChain chain, string botAccount)
    {
        var ctx = new MessageContext(
            botAccount,
            "SendGroupMessage",
            Constant.QqPlatformType,
            MessageType.GroupMessage,
            chain.TargetId,
            chain.PlatformId ?? throw new ArgumentException("PlatformId is null, however invoke SendGroupMessageAsync"),
            chain.TargetId,
            chain,
            DateTime.Now
        );
        return api.SendMessage(ctx);
    }
    
    public static async Task<bool> QqKickGroupMemberAsync(this ICommonApi api, string botAccount, string targetId, string targetPlatformId, bool isRejectAgain)
    {
        var ctx = new MessageContext(
            botAccount,
            "KickGroupMember",
            Constant.QqPlatformType,
            MessageType.Notify,
            targetId,
            targetPlatformId,
            targetPlatformId,
            null,
            DateTime.Now
        );
        ctx.UnderProperty.TryAdd("RejectAgain", isRejectAgain.ToString());
        var res = await api.SendMessageAsync(ctx);
        return res.UnderProperty["KickResult"] == "true";
    }
    
    public static IResponsePromise QqKickGroupMember(this ICommonApi api, string botAccount, string targetId, string targetPlatformId, bool isRejectAgain)
    {
        var ctx = new MessageContext(
            botAccount,
            "KickGroupMember",
            Constant.QqPlatformType,
            MessageType.Notify,
            targetId,
            targetPlatformId,
            targetPlatformId,
            null,
            DateTime.Now
        );
        ctx.UnderProperty.TryAdd("RejectAgain", isRejectAgain.ToString());
        return api.SendMessage(ctx);
    }
    
    public static async Task<bool> QqKickGroupMemberWithCurrentContextAsync(this ICommonApi api, MessageContext context, bool isRejectAgain)
    {
        var ctx = new MessageContext(
            context.BotAccount,
            "KickGroupMember",
            Constant.QqPlatformType,
            MessageType.Notify,
            context.TriggerId,
            context.TriggerPlatformId,
            context.TriggerPlatformId,
            null,
            DateTime.Now
        );
        ctx.UnderProperty.TryAdd("RejectAgain", isRejectAgain.ToString());
        var res = await api.SendMessageAsync(ctx);
        return res.UnderProperty["KickResult"] == "true";
    }
    
    public static IResponsePromise QqKickGroupMemberWithCurrentContext(this ICommonApi api, MessageContext context, bool isRejectAgain)
    {
        var ctx = new MessageContext(
            context.BotAccount,
            "KickGroupMember",
            Constant.QqPlatformType,
            MessageType.Notify,
            context.TriggerId,
            context.TriggerPlatformId,
            context.TriggerPlatformId,
            null,
            DateTime.Now
        );
        ctx.UnderProperty.TryAdd("RejectAgain", isRejectAgain.ToString());
        return api.SendMessage(ctx);
    }
    
    public static void QqReadNextGroupMessageAsync(this ICommonApi api, string triggerId, string triggerPlatformId, 
        Func<MessageContext, PluginFlag> success,
        Func<MessageContext, PluginFlag> failure,
        int timeOut = 60)
    {
        var listener = new PluginsListenerDescriptor(
            MessageType.GroupMessage,
            Constant.QqPlatformType,
            "SendGroupMessage",
            ctx => ctx.TriggerId == triggerId && ctx.TriggerPlatformId == triggerPlatformId,
            DateTime.Now.AddSeconds(timeOut),
            true,
            true,
            success,
            failure
        );
        
        api.RegisterListener(listener);
    }
    
    public static void QqReadNextPrivateMessageAsync(this ICommonApi api, string triggerId,
        Func<MessageContext, PluginFlag> success,
        Func<MessageContext, PluginFlag> failure,
        int timeOut = 60)
    {
        api.RegisterListener(new PluginsListenerDescriptor(
            MessageType.GroupMessage,
            Constant.QqPlatformType,
            "SendFriendMessage",
            ctx => ctx.TriggerId == triggerId,
            DateTime.Now.AddSeconds(timeOut),
            true,
            true,
            success,
            failure
        ));
    }
}