namespace SoruxBot.SDK.QQ.Entity;

public class MentionEntity 
{
    public uint Uin { get; set; }

    public string Uid { get; set; }

    public string? Name { get; set; }

    public MentionEntity()
    {
        Uin = 0;
        Uid = "";
        Name = "";
    }

    /// <summary>
    /// Set target to 0 to mention everyone
    /// </summary>
    public MentionEntity(string? name, uint target = 0)
    {
        Uin = target;
        Uid = ""; // automatically resolved by MessagingLogic.cs
        Name = name;
    }

    public string ToPreviewString()
    {
        return $"[Mention]: {Name}({Uin})";
    }

    public string ToPreviewText() => $"{Name} ";
}