using Grpc.Core;
using SoruxBot.Kernel.Constant;
using SoruxBot.Provider.WebGrpc;

namespace SoruxBot.Wrapper.Service;

public class TracingMessageClient(Message.MessageClient client) : Message.MessageClient
{
    public override MessageResponse MessageSend(MessageRequest request, CallOptions options)
    {
        using var activity = OpenTelemetryHelper.ActivitySource.StartActivity();
        return client.MessageSend(request, options);
    }
}