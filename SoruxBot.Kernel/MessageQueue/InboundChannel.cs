using System.Threading.Channels;
using SoruxBot.SDK.Model.Message;

namespace SoruxBot.Kernel.MessageQueue;

public class InboundChannel
{
    private readonly Channel<MessageContext> _channel = Channel.CreateUnbounded<MessageContext>();
    
    public async Task<MessageContext?> GetNextMessageRequest()
    {
        return await _channel.Reader.ReadAsync();
    }

    public async void SetNextMsg(MessageContext value)
    {
        await _channel.Writer.WriteAsync(value);
    }
}