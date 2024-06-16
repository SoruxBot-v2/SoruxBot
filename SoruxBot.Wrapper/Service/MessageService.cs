using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Newtonsoft.Json;
using SoruxBot.Kernel.Constant;
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
        using var activity = OpenTelemetryHelper.ActivitySource.StartActivity();
        
        if (request.Token != token)
        {
            return Task.FromResult(new Empty());
        }
        
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        try
        {
            var messageContext = JsonConvert.DeserializeObject<MessageContext>(request.Payload, settings)!;
            message.SetNextMsg(messageContext);
        }
        catch (Exception e)
        {
            loggerService.Warn("MessageService", "Deserialize message failed: " + e.Message);
        }
        
        return Task.FromResult(new Empty());
    }
}