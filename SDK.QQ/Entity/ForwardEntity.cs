namespace SoruxBot.SDK.QQ.Entity;

public class ForwardEntity
{
    public DateTime Time { get; set; }

    public ulong MessageId { get; set; }

    public uint Sequence { get; set; }

    public string? Uid { get; set; }

    public uint TargetUin { get; set; }

    private string? SelfUid { get; set; }

    public ForwardEntity()
    {
        Sequence = 0;
        Uid = null;
        Elements = new List<Elem>();
    }

    public ForwardEntity(MessageChain chain)
    {
        Time = chain.Time;
        Sequence = chain.Sequence;
        Uid = chain.Uid;
        Elements = chain.Elements;
        TargetUin = chain.FriendUin;
        MessageId = chain.MessageId;
    }

    public void SetSelfUid(string selfUid) => SelfUid = selfUid;

    public string ToPreviewString() => $"[Forward]: Sequence: {Sequence}";

    public string ToPreviewText() => string.Empty;
}