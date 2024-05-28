using SoruxBot.SDK.Model.Message.Entity;

namespace SoruxBot.SDK.Model.Message;

public class MessageBuilder(MessageChain messageChain)
{
    public static MessageBuilder GroupMessage(string platformId, string platformType) => new(new MessageChain(String.Empty, String.Empty, platformId, platformId, platformType));
    
    public static MessageBuilder PrivateMessage(string targetId, string platformType) => new(new MessageChain(String.Empty, targetId, targetId, targetId, platformType));
    
    public MessageBuilder Text(string content)
    {
        messageChain.Messages.Add(new TextMessage(content));
        return this;
    }
    
    public MessageBuilder CommonMessage(string type, Dictionary<string, object> content)
    {
        messageChain.Messages.Add(new CommonMessage(type));
        return this;
    }
    
    public virtual MessageChain Build() { return messageChain; }
}