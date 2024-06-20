using SoruxBot.SDK.Model.Message.Entity;
using SoruxBot.SDK.Model.Message;

namespace SoruxBot.SDK.QQ.Entity;

public class MultiMsgMessage(uint? groupUin, List<MessageChain> chains) : CommonMessage("multiMsg")

{
    /// <summary>
    /// 转发源群的Uin
    /// </summary>
    public uint? GroupUin { get; set; } = groupUin;

    /// <summary>
    /// 合并转发包含的消息链们
    /// </summary>
    public List<MessageChain> Chains { get; } = chains;

    public override string ToPreviewText() => Convert.ToString(groupUin);
}