using System.Threading.Channels;
using SoruxBot.Kernel.Interface;
using SoruxBot.SDK.Model.Message;

namespace SoruxBot.Kernel.MessageQueue;

public class MessageQueue : IMessageQueue
{
    private readonly Channel<MessageContext> _channel = Channel.CreateUnbounded<MessageContext>();
    
    

    public async Task<MessageContext> GetNextMessageRequest()
    {
        // return  _channel.Reader.TryRead(out var res) ? res : null;
     return  await _channel.Reader.ReadAsync();
    }

    public void SetNextMsg(MessageContext value)
    {
        while (!_channel.Writer.TryWrite(value))
        {
        }
    }
}