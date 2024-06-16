using System.Threading.Channels;
using SoruxBot.Kernel.Constant;
using SoruxBot.Kernel.Interface;
using SoruxBot.SDK.Model.Message;

namespace SoruxBot.Kernel.MessageQueue;

public class MessageQueue : IMessageQueue
{
    private readonly Channel<MessageContext> _channel = Channel.CreateUnbounded<MessageContext>();
    
    

    public async Task<MessageContext> GetNextMessageRequest()
    {
        using var activity = OpenTelemetryHelper.ActivitySource.StartActivity();
        return  await _channel.Reader.ReadAsync();
    }

    public void SetNextMsg(MessageContext value)
    {
        using var activity = OpenTelemetryHelper.ActivitySource.StartActivity();
        while (!_channel.Writer.TryWrite(value))
        {
        }
    }
}