using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Newtonsoft.Json;
using SoruxBot.Kernel.Interface;
using SoruxBot.Provider.WebGrpc;
using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Plugins.Service;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
        
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };
        
        message.SetNextMsg(JsonConvert.DeserializeObject<MessageContext>(request.Payload, settings));
        return Task.FromResult(new Empty());
    }
}