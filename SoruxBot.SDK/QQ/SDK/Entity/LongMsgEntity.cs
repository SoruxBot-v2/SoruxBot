namespace SoruxBot.SDK.QQ.SDK.Entity;

public class LongMsgEntity
{
    public string ResId { get; internal set; }

    public MessageChain Chain { get; internal set; }

    internal LongMsgEntity()
    {
        ResId = string.Empty;
        Chain = MessageBuilder.Group(0).Build();
    }

    public LongMsgEntity(string resId)
    {
        ResId = resId;
        Chain = MessageBuilder.Group(0).Build();
    }

    public LongMsgEntity(MessageChain chain)
    {
        ResId = string.Empty;
        Chain = chain;
    }

    public string ToPreviewString() => $"[{nameof(LongMsgEntity)}] {Chain.ToPreviewString()}";
}