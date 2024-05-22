namespace SoruxBot.SDK.QQ.Entity;

public class PokeEntity
{
    public uint Type { get; }

    public PokeEntity(uint type)
    {
        Type = type;
    }

    public string ToPreviewString() => $"[{nameof(PokeEntity)}: {Type}]";
}