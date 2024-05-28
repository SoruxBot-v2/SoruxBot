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
            "QQ",
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
            "QQ",
            MessageType.PrivateMessage,
            chain.TargetId,
            chain.TargetId,
            chain.TargetId,
            chain,
            DateTime.Now
        );
        return api.SendMessage(ctx);
    }
}