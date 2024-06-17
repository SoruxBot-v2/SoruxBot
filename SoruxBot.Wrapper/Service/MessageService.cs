using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;
using SoruxBot.Kernel.Constant;
using SoruxBot.Kernel.Interface;
using SoruxBot.Provider.WebGrpc;
using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Plugins.Service;
using SoruxBot.Kernel.Bot;
using SoruxBot.Kernel.Services.PluginService.JsonConvertService;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SoruxBot.Wrapper.Service;

public class MessageService(BotContext botContext, ILoggerService loggerService, IMessageQueue message, string? token) : Message.MessageBase
{
    public override Task<Empty> MessagePushStack(MessageRequest request, ServerCallContext context)
    {
        loggerService.Info("MessageService", "Catch msg: " + request.Payload);
        using var activity = OpenTelemetryHelper.ActivitySource.StartActivity();
        
        if (request.Token != token)
        {
            return Task.FromResult(new Empty());
        }

		var settings = new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.All 
		};

		var messageJsonConvert = botContext.ServiceProvider.GetRequiredService<JsonConvertMap>();
		var platform = (string)JObject.Parse(request.Payload)["TargetPlatform"]!;
		if (!messageJsonConvert.TryGet(platform, out var jsonConvert))
		{
			loggerService.Warn("Parsing message", "Cannot find jsonConvert of platform " + platform);
			try
			{
				message.SetNextMsg(JsonConvert.DeserializeObject<MessageContext>(request.Payload, settings)!);
			}
			catch (Exception ex)
			{
				loggerService.Error("Parsing message", ex.Message);
				loggerService.Error("Parsing message", "Failed to parse message: " + request.Payload);
			}
		}
        message.SetNextMsg(jsonConvert!.DeserializeObject<MessageContext>(request.Payload));
        return Task.FromResult(new Empty());
    }
}