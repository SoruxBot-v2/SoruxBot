using SoruxBot.SDK.Model.Message;

namespace SoruxBot.SDK.Attribute;

[AttributeUsage(AttributeTargets.Method)]
public class MessageEventAttribute(MessageType eventType) : System.Attribute
{
    public MessageType MessageType { get; init; } = eventType;
}