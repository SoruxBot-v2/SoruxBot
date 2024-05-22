namespace SoruxBot.SDK.QQ.SDK.Entity;

public class FaceEntity
{
    public ushort FaceId { get; }

    public bool IsLargeFace { get; }

    public FaceEntity() { }

    public FaceEntity(ushort faceId, bool isLargeFace)
    {
        FaceId = faceId;
        IsLargeFace = isLargeFace;
    }

    public string ToPreviewString() => $"[Face][{(IsLargeFace ? "Large" : "Small")}]: {FaceId}";
}