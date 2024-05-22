namespace SoruxBot.SDK.QQ.Entity;

public class MultiMsgEntity
{
    public uint? GroupUin { get; set; }

    public List<MessageChain> Chains { get; }

    public string ToPreviewString() => $"[MultiMsgEntity] {Chains.Count} chains";

    public string ToPreviewText() => "[聊天记录]";
}