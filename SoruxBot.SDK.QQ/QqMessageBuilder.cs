using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.QQ.Entity;

namespace SoruxBot.SDK.QQ;

public class QqMessageBuilder(MessageChain messageChain) : MessageBuilder(messageChain)
{
    public static MessageBuilder GroupMessage(string platformId) => new(new MessageChain(String.Empty, String.Empty, platformId, platformId, Constant.QqPlatformType));

    public static MessageBuilder PrivateMessage(string targetId) => new(new MessageChain(String.Empty, targetId, targetId, targetId, Constant.QqPlatformType));

    public QqMessageBuilder Face(ushort faceId, bool isLargeFace = false)
    {
        messageChain.Messages.Add(new FaceMessage(faceId, isLargeFace));
        return this;
    }

    public QqMessageBuilder Mention(string? name, uint target = 0)
    {
        messageChain.Messages.Add(new MentionMessage(name, target));
        return this;
    }

    public QqMessageBuilder Poke(uint type)
    {
        messageChain.Messages.Add(new PokeMessage(type));
        return this;
    }

    public QqMessageBuilder ImageOfUrl(string imageUrl)
    {
        messageChain.Messages.Add(new ImageMessage(imageUrl));
        return this;
    }

    public QqMessageBuilder ImageOfFilePath(string filePath)
    {
        messageChain.Messages.Add(new ImageMessage(null, filePath));
        return this;
    }

    public QqMessageBuilder ImageOfBytes(byte[] imageBytes)
    {
        messageChain.Messages.Add(new ImageMessage(imageBytes));
        return this;
    }

    public QqMessageBuilder RecordOfUrl(string audioUrl)
    {
        messageChain.Messages.Add(new RecordMessage(audioUrl));
        return this;
    }

    public QqMessageBuilder RecordOfFilePath(string filePath)
    {
        messageChain.Messages.Add(new RecordMessage(null, filePath));
        return this;
    }

    public QqMessageBuilder RecordOfBytes(byte[] recordBytes)
    {
        messageChain.Messages.Add(new RecordMessage(recordBytes));
        return this;
    }

    public QqMessageBuilder VideoOfUrl(string videoUrl)
    {
        messageChain.Messages.Add(new VideoMessage(videoUrl));
        return this;
    }

    public QqMessageBuilder VideoOfFilePath(string filePath)
    {
        messageChain.Messages.Add(new VideoMessage(null, filePath));
        return this;
    }

    public QqMessageBuilder VideoOfBytes(byte[] videoBytes)
    {
        messageChain.Messages.Add(new VideoMessage(videoBytes));
        return this;
    }

    public QqMessageBuilder Forward(uint sequence, MessageChain messageChain)
    {
        messageChain.Messages.Add(new ForwardMessage(sequence, messageChain));
        return this;
    }

    public QqMessageBuilder LongMsg(string resId)
    {
        messageChain.Messages.Add(new LongMsgMessage(resId, QqMessageBuilder.GroupMessage("0").Build()));
        return this;
    }

    public QqMessageBuilder LongMsg(MessageChain messageChain)
    {
        messageChain.Messages.Add(new LongMsgMessage(string.Empty, messageChain));
        return this;
    }

    public QqMessageBuilder MultiMsg(uint? groupUin = null, params MessageChain[] chains)
    {
        messageChain.Messages.Add(new MultiMsgMessage(groupUin, chains.ToList()));
        return this;
    }
}