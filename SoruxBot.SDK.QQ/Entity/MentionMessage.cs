using SoruxBot.SDK.Model.Message.Entity;

namespace SoruxBot.SDK.QQ.Entity;

public class MentionMessage(string? name, uint target = 0) : CommonMessage("mention")
{
    /// <summary>
    /// 提及用户的 Uin
    /// </summary>
    public uint Uin { get; set; } = target;

    /// <summary>
    /// 提及用户的 Uid
    /// </summary>
    public string Uid { get; set; } = "";

    /// <summary>
    /// 提及用户的昵称
    /// </summary>
    public string? Name { get; set; } = name;

    public override string ToPreviewText() => Convert.ToString(Uin);
}