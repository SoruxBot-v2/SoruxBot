using SoruxBot.SDK.Model.Message.Entity;

namespace SoruxBot.SDK.QQ.Entity;

public class FaceMessage(ushort faceId, bool isLargeFace) : CommonMessage("face")

{
    /// <summary>
    /// 表情ID
    /// </summary>
    public ushort FaceId { get; } = faceId;

    /// <summary>
    /// 是否为大表情
    /// </summary>
    public bool IsLargeFace { get; } = isLargeFace;

    public override string ToPreviewText() => Convert.ToString(FaceId);
}