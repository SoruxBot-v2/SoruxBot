using SoruxBot.SDK.Model.Message.Entity;

namespace SoruxBot.SDK.QQ.Entity;

public class PokeMessage(uint type) : CommonMessage("poke")
{
    /// <summary>
    /// 戳一戳的类型
    /// </summary>
    public uint PokeType { get; } = type;

    public string ToPreviewString() => Convert.ToString(PokeType);
}