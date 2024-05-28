using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using SoruxBot.Kernel.Interface;
using SoruxBot.Provider.WebGrpc;
using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Plugins.Service;

namespace SoruxBot.Wrapper.Service;

public class MessageService(ILoggerService loggerService, IMessageQueue message, string? token) : Message.MessageBase
{
    public override Task<Empty> MessagePushStack(MessageRequest request, ServerCallContext context)
    {
        loggerService.Info("MessageService", "Catch msg: " + request.Payload);
        if (request.Token != token)
        {
            return Task.FromResult(new Empty());
        }
        
        message.SetNextMsg(JsonSerializer.Deserialize<MessageContext>(request.Payload));
        return Task.FromResult(new Empty());
    }
}