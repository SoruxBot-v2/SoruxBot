using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Model.Message.Entity;

namespace SoruxBot.SDK.QQ.Entity;

public class ForwardMessage : CommonMessage

{
    /// <summary>
    /// 回复的目标消息Seq
    /// </summary>
    public uint Sequence { get; set; }

    /// <summary>
    /// 回复的消息
    /// </summary>
    public MessageChain Target { get; set; }

    public ForwardMessage(uint sequence) : base("forward")
    {
        Sequence = sequence;
        Target = new MessageChain(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
    }

    public ForwardMessage(uint sequence, MessageChain target) : base("forward")
    {
        Sequence = sequence;
        Target = target;
    }

    public override string ToPreviewText() => Convert.ToString(Target.SelfId);
}