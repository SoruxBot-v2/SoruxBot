using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Plugins.Service;

namespace SoruxBot.SDK.QQ;

public static class QqExternApi
{
    public static Task<MessageResult> SendFriendMessage(this ICommonApi api, MessageChain chain, string botAccount)
    {
        var ctx = new MessageContext(
            botAccount,
            "sendFriendMessage",
            "QQ",
            MessageType.PrivateMessage,
            chain.TargetId,
            chain.PlatformId ?? "",
            chain.TiedId ?? "",
            chain,
            DateTime.Now
            );
        return api.SendMessageAsync(ctx);
    }
}