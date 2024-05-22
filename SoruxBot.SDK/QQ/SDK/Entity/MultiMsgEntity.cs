namespace SoruxBot.SDK.QQ.SDK.Entity;

public class MultiMsgEntity
{
    private static readonly XmlSerializer Serializer = new(typeof(MultiMessage));

    public uint? GroupUin { get; set; }

    public List<MessageChain> Chains { get; }

    public string ToPreviewString() => $"[MultiMsgEntity] {Chains.Count} chains";

    public string ToPreviewText() => "[ÁÄÌì¼ÇÂ¼]";
}