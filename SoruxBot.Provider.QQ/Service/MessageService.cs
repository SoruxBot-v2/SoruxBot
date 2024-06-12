using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Lagrange.Core;
using Lagrange.Core.Common.Interface.Api;
using Microsoft.AspNetCore.SignalR.Protocol;
using Newtonsoft.Json;
using SoruxBot.Provider.WebGrpc;
using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Model.Message.Entity;
using SoruxBot.SDK.QQ.Entity;
using JsonSerializer = System.Text.Json.JsonSerializer;
using MessageResult = Lagrange.Core.Message.MessageResult;

namespace SoruxBot.Provider.QQ.Service;

public class MessageService(string? token, BotContext bot) : Message.MessageBase
{
    public override async Task<MessageResponse> MessageSend(MessageRequest request, ServerCallContext context)
    {
        var response = new MessageResponse();
        
        if (request.Token != token)
        {
            // 表示无回应
            response.Payload = string.Empty;
            return await Task.FromResult(response);
        }
        
        // 这个进行路由处理
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };
        
        var messageContext = JsonConvert.DeserializeObject<MessageContext>(request.Payload, settings);
        if (messageContext is null)
        {
            // 表示无回应
            response.Payload = string.Empty;
            return await Task.FromResult(response);
        }

        
        var result = await DispatchMessage(messageContext);
		var res = new SoruxBot.SDK.Model.Message.MessageResult(
				result.Sequence?.ToString() ?? "0",
				DateTime.Now
			);

        response.Payload = JsonConvert.SerializeObject(res, settings);
        return await Task.FromResult(response);
    }

    private async Task<MessageResult> DispatchMessage(MessageContext ctx)
    {
        // TODO 补全所有的 Action
        switch (ctx.TargetPlatformAction)
        {
            case "FriendMessage":
            {
                // 处理好友消息
                var msg = Lagrange.Core.Message.MessageBuilder
                    .Friend(uint.Parse(ctx.MessageChain!.TargetId));
                msg = ConvertMessageBuilder(msg, ctx);
                var result = await bot.SendMessage(msg.Build());
                return result;
            }
            case "GroupMessage":
            {
                // 处理群聊消息
                var msg = Lagrange.Core.Message.MessageBuilder
                    .Group(uint.Parse(ctx.MessageChain!.PlatformId!));
                msg = ConvertMessageBuilder(msg, ctx);
                var result = await bot.SendMessage(msg.Build());
                return result;
            }
        }
        
        return new MessageResult();
    }

    private Lagrange.Core.Message.MessageBuilder 
        ConvertMessageBuilder(Lagrange.Core.Message.MessageBuilder builder, MessageContext ctx)
    {
        // TODO 转换和适配为 Provider 认可的消息链
        foreach (var msg in ctx.MessageChain!.Messages)
        {
            if (msg is TextMessage textMessage)
            {
                builder.Text(textMessage.Content);
            }

            else if (msg is FaceMessage faceMessage)
            {
                builder.Face(faceMessage.FaceId);
            }

            else if (msg is MentionMessage mentionMessage)
            {
                builder.Mention(mentionMessage.Uin);
            }

            else if (msg is PokeMessage pokeMessage)
            {
                builder.Poke(pokeMessage.PokeType);
            }

            else if (msg is ImageMessage imageMessage)
            {
                if (imageMessage.FilePath != null)
                {
                    builder.Image(imageMessage.FilePath);
                }
                else
                {
                    builder.Image(imageMessage.ImageBytes);
                }
            }

            else if (msg is RecordMessage recordMessage)
            {
                if (recordMessage.FilePath != null)
                {
                    builder.Image(recordMessage.FilePath);
                }
                else
                {
                    builder.Image(recordMessage.AudioBytes);
                }
            }

            else if (msg is VideoMessage videoMessage)
            {
                if (videoMessage.FilePath != null)
                {
                    builder.Image(videoMessage.FilePath);
                }
                else
                {
                    builder.Image(videoMessage.VideoBytes);
                }
            }
        }

        return builder;
    }
}