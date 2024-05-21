namespace SoruxBot.SDK.QQ.SDK.Entity;

public class MarketFaceEntity 
{
    public string FaceId { get; }

    public int TabId { get; }

    public string Key { get; }

    public string Summary { get; }

    public MarketFaceEntity() : this(string.Empty, default, string.Empty, string.Empty) { }

    public MarketFaceEntity(string faceId, int tabId, string key, string summary)
    {
        FaceId = faceId;
        TabId = tabId;
        Key = key;
        Summary = summary;
    }

    public string ToPreviewString()
    {
        return $"[{nameof(MarketFaceEntity)}: TabId: {TabId}; FaceId: {FaceId}; Key: {Key}; Summary: {Summary}]";
    }
}