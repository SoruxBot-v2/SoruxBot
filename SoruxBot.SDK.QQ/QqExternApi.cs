using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Plugins.Service;

namespace SoruxBot.SDK.QQ;

public static class QqExternApi
{
    // TODO 完成剩下的 API
    public static Task<MessageResult> SendFriendMessageAsync(this ICommonApi api, MessageChain chain, string botAccount)
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
    
    public static IResponsePromise SendFriendMessage(this ICommonApi api, MessageChain chain, string botAccount)
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
    
    public static async Task<bool> KickGroupMemberAsync(this ICommonApi api, string botAccount, string targetId, string targetPlatformId, bool isRejectAgain)
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
    
    public static IResponsePromise KickGroupMember(this ICommonApi api, string botAccount, string targetId, string targetPlatformId, bool isRejectAgain)
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
    
    public static async Task<bool> KickGroupMemberWithCurrentContextAsync(this ICommonApi api, MessageContext context, bool isRejectAgain)
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
    
    public static IResponsePromise KickGroupMemberWithCurrentContext(this ICommonApi api, MessageContext context, bool isRejectAgain)
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
}