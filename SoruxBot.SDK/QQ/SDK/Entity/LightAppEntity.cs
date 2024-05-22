namespace SoruxBot.SDK.QQ.SDK.Entity;

public class LightAppEntity
{
    public string AppName { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public LightAppEntity() { }

    public LightAppEntity(string payload)
    {
        Payload = payload;
        string? app = JsonNode.Parse(payload)?["app"]?.ToString();
        if (app != null) AppName = app;
    }

    public string ToPreviewString()
    {
        return $"[{nameof(LightAppEntity)}: {AppName}]";
    }
}