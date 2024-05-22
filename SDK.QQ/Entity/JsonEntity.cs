namespace SoruxBot.SDK.QQ.Entity;

public class JsonEntity
{
    public string Json { get; set; }

    public string ResId { get; set; }

    public JsonEntity()
    {
        Json = "";
        ResId = "";
    }

    public JsonEntity(string json, string resId = "")
    {
        Json = json;
        ResId = resId;
    }

    public JsonEntity(JsonNode json, string resId = "")
    {
        Json = json.ToJsonString();
        ResId = resId;
    }

    public string ToPreviewString()
    {
        throw new NotImplementedException();
    }
}