using SoruxBot.SDK.Model.Message.Entity;

namespace SoruxBot.SDK.QQ.Entity;

public class MentionMessage(string? name, uint target = 0) : CommonMessage("mention")

{
    public uint Uin { get; set; } = target;

    public string Uid { get; set; } = "";

    public string? Name { get; set; } = name;


    public override string ToPreviewText() => $"[Mention] {Name} ";
}