namespace SoruxBot.SDK.QQ.SDK.Entity;

public class RecordEntity 
{
    public int AudioLength { get; set; }

    public string FilePath { get; set; } = string.Empty;

    public string AudioName { get; set; } = string.Empty;

    public int AudioSize => (int?)AudioStream?.Value.Length ?? default;

    public string AudioUrl { get; set; } = string.Empty;

    public RecordEntity(string filePath, int audioLength = 0)
    {
        FilePath = filePath;
        AudioStream = new Lazy<Stream>(() => new FileStream(filePath, FileMode.Open, FileAccess.Read));
        AudioLength = audioLength;
    }

    public RecordEntity(byte[] file, int audioLength = 0)
    {
        FilePath = string.Empty;
        AudioStream = new Lazy<Stream>(() => new MemoryStream(file));
        AudioLength = audioLength;
    }

    public string ToPreviewString() => $"[{nameof(RecordEntity)}: {AudioUrl}]";

    public string ToPreviewText() => "[”Ô“Ù]";
}
