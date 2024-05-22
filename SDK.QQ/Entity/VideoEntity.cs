namespace SoruxBot.SDK.QQ.Entity;

public class VideoEntity
{
    public string FilePath { get; set; } = string.Empty;

    public string VideoHash { get; set; } = string.Empty;

    public Vector2 Size { get; }

    public int VideoSize { get; set; }

    public int VideoLength { get; set; }

    public string VideoUrl { get; set; } = string.Empty;

    public VideoEntity(string filePath, int videoLength = 0)
    {
        FilePath = filePath;
        VideoStream = new Lazy<Stream>(() => new FileStream(filePath, FileMode.Open, FileAccess.Read));
        VideoLength = videoLength;
    }

    public VideoEntity(byte[] file, int videoLength = 0)
    {
        FilePath = string.Empty;
        VideoStream = new Lazy<Stream>(() => new MemoryStream(file));
        VideoLength = videoLength;
    }

    public string ToPreviewString() => $"[Video {Size.X}x{Size.Y}]: {VideoSize} {VideoUrl}";

    public string ToPreviewText() => "[视频]";
}
