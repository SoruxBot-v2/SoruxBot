using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Lagrange.Core;
using Lagrange.Core.Common.Interface.Api;
using SoruxBot.Provider.WebGrpc;
using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Model.Message.Entity;
using MessageResult = Lagrange.Core.Message.MessageResult;

namespace SoruxBot.Provider.QQ.Service;

public class MessageService(string? token, BotContext bot) : Message.MessageBase
{
    public override Task<MessageResponse> MessageSend(MessageRequest request, ServerCallContext context)
    {
        var response = new MessageResponse();
        
        if (request.Token != token)
        {
            // 表示无回应
            response.Payload = string.Empty;
            return Task.FromResult(response);
        }
        
        // 这个进行路由处理
        var messageContext = JsonSerializer.Deserialize<MessageContext>(request.Payload);
        if (messageContext is null)
        {
            // 表示无回应
            response.Payload = string.Empty;
            return Task.FromResult(response);
        }
        
        var result = DispatchMessage(messageContext);
        response.Payload = JsonSerializer.Serialize(result);
        return Task.FromResult(response);
    }

    private async Task<MessageResult> DispatchMessage(MessageContext ctx)
    {
        switch (ctx.TargetPlatformAction)
        {
            case "FriendMessage":
                // 处理好友消息
                var msg = Lagrange.Core.Message.MessageBuilder
                    .Friend(uint.Parse(ctx.MessageChain!.TargetId));
                msg = ConvertMessageBuilder(msg, ctx);
                var result = await bot.SendMessage(msg.Build());
                return result;
        }
        
        return new MessageResult();
    }

    private Lagrange.Core.Message.MessageBuilder 
        ConvertMessageBuilder(Lagrange.Core.Message.MessageBuilder builder, MessageContext ctx)
    {
        foreach (var msg in ctx.MessageChain!.Messages)
        {
            if (msg is TextMessage textMessage)
            {
                builder.Text(textMessage.Content);
            }
        }

        return builder;
    }
}