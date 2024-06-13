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

        
        var res = await DispatchMessage(messageContext);

        response.Payload = JsonConvert.SerializeObject(res, settings);
        return await Task.FromResult(response);
    }

    private async Task<SoruxBot.SDK.Model.Message.MessageResult> DispatchMessage(MessageContext ctx)
    {
        // TODO 补全所有的 Action
        switch (ctx.TargetPlatformAction)
        {
            case "SendFriendMessage":
            {
                // 处理好友消息
                var msg = Lagrange.Core.Message.MessageBuilder
                    .Friend(uint.Parse(ctx.MessageChain!.TargetId));
                msg = ConvertMessageBuilder(msg, ctx);
                var result = await bot.SendMessage(msg.Build());
                return new SoruxBot.SDK.Model.Message.MessageResult(
                    result.Sequence?.ToString() ?? "0",
                    DateTime.Now
                );
            }
            case "SendGroupMessage":
            {
                // 处理群聊消息
                var msg = Lagrange.Core.Message.MessageBuilder
                    .Group(uint.Parse(ctx.MessageChain!.PlatformId!));
                msg = ConvertMessageBuilder(msg, ctx);
                var result = await bot.SendMessage(msg.Build());
                return new SoruxBot.SDK.Model.Message.MessageResult(
                    result.Sequence?.ToString() ?? "0",
                    DateTime.Now
                );
            }
            case "KickGroupMember":
            {
                var res = new SoruxBot.SDK.Model.Message.MessageResult(
                    "-1",
                    DateTime.Now
                );
                
                if (!uint.TryParse(ctx.TriggerPlatformId, out var platformId))
                {
                    return res;
                }
                
                if (!uint.TryParse(ctx.TriggerId, out var triggerId))
                {
                    return res;
                }
                
                var result = await bot.KickGroupMember(platformId, triggerId, ctx.UnderProperty["RejectAgain"] == "true");
                res = new SoruxBot.SDK.Model.Message.MessageResult(
                    "0",
                    DateTime.Now
                );
                res.UnderProperty.Add("KickResult", result.ToString());
                return res;
            }
        }
        
        return new SoruxBot.SDK.Model.Message.MessageResult(
            "0",
            DateTime.Now);
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