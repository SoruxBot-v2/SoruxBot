using SoruxBot.SDK.Model.Message;

namespace SoruxBot.SDK.QQ;

public class QqMessageBuilder(MessageChain messageChain) : MessageBuilder(messageChain)
{
    public static MessageBuilder GroupMessage(string platformId) => new(new MessageChain(String.Empty, String.Empty, platformId, platformId, Constant.QqPlatformType));
    
    public static MessageBuilder PrivateMessage(string targetId) => new(new MessageChain(String.Empty, targetId, targetId, targetId, Constant.QqPlatformType));
}