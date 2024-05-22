using SoruxBot.Kernel.Interface;
using SoruxBot.SDK.Model.Message;

namespace SoruxBot.Kernel.MessageQueue;

public class InboundChannelWrapper : IMessageQueue
{
    private readonly InboundChannel _channel = new();

    public MessageContext? GetNextMessageRequest()
    {
        return _channel.GetNextMessageRequest().Result;
    }

    public void SetNextMsg(MessageContext value)
    {
        _channel.SetNextMsg(value);
    }
}