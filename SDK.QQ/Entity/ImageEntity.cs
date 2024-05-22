namespace SoruxBot.SDK.QQ.Entity;

public class ImageEntity
{
    private const string BaseUrl = "https://multimedia.nt.qq.com.cn";

    private const string LegacyBaseUrl = "http://gchat.qpic.cn";

    public Vector2 PictureSize { get; set; }

    public string FilePath { get; set; } = string.Empty;

    public uint ImageSize { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public ImageEntity() { }

    public ImageEntity(string filePath)
    {
        FilePath = filePath;
        ImageStream = new Lazy<Stream>(() => new FileStream(filePath, FileMode.Open, FileAccess.Read));
    }

    public ImageEntity(byte[] file)
    {
        FilePath = "";
        ImageStream = new Lazy<Stream>(() => new MemoryStream(file));
    }

    public ImageEntity(Stream stream)
    {
        FilePath = "";
        ImageStream = new Lazy<Stream>(stream);
    }

    public string ToPreviewString() => $"[Image: {PictureSize.X}x{PictureSize.Y}] {ToPreviewText()} {FilePath} {ImageSize} {ImageUrl}";

    public string ToPreviewText() => string.IsNullOrEmpty(Summary)
        ? SubType switch
        {
            1 => "[动画表情]",
            _ => "[图片]",
        }
        : Summary;
}