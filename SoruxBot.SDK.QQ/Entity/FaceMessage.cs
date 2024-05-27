using SoruxBot.SDK.Model.Message.Entity;

namespace SoruxBot.SDK.QQ.Entity;

public class FaceMessage(ushort faceId, bool isLargeFace) : CommonMessage("face")

{
    public ushort FaceId { get; } = faceId;

    public bool IsLargeFace { get; } = isLargeFace;
    
    public override string ToPreviewText() => $"[Face][{(IsLargeFace ? "Large" : "Small")}]: {FaceId}";
}