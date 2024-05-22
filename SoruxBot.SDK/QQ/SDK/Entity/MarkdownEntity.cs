namespace SoruxBot.SDK.QQ.SDK.Entity;

public class MarkdownEntity 
{
    public MarkdownData Data { get; set; }

    public MarkdownEntity(MarkdownData data) => Data = data;

    public MarkdownEntity(string data) => Data = JsonSerializer.Deserialize<MarkdownData>(data) ?? throw new Exception();

    public string ToPreviewString() => throw new NotImplementedException();
}