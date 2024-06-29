using SoruxBot.SDK.Model.Message.Entity;
using SoruxBot.SDK.Model.Message;

using SoruxBot.SDK.Model.Message.Entity;

namespace SoruxBot.SDK.QQ.Entity;

public class LongMsgMessage(string resId, MessageChain chain) : CommonMessage("longMsg")

{
    /// <summary>
    /// 长消息标识Id
    /// </summary>
    public string ResId { get; set; } = resId;

    /// <summary>
    /// 长消息
    /// </summary>
    public MessageChain Chain { get; set; } = chain;

    public override string ToPreviewText() => Convert.ToString(resId);
}