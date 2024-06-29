using SoruxBot.SDK.Model.Message.Entity;
using System.Net;
using System.Numerics;

namespace SoruxBot.SDK.QQ.Entity;

public class ImageMessage : CommonMessage

{
    /// <summary>
    /// 图片大小（长宽）
    /// </summary>
    public Vector2 PictureSize { get; set; }

    /// <summary>
    /// 图片的本地路径
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// 图片字节大小
    /// </summary>
    public uint ImageSize { get; set; }

    /// <summary>
    /// 图片Url
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// 图片字节数据
    /// </summary>
    public byte[]? ImageBytes { get; set; }

    public ImageMessage(string? imageUrl = null, string? filePath = null, byte[]? imageBytes = null, float PicWidth = 0, float PicHeight = 0, uint imageSize = 0) : base("image")
    {
        PictureSize = new Vector2(PicWidth, PicHeight);
        FilePath = filePath;
        ImageUrl = imageUrl;
        if (imageBytes != null) ImageBytes = imageBytes;
        else if (filePath != null)
        {
            ImageBytes = GetImageBytesFromFilePath(filePath);
        }
        else if (imageUrl != null)
        {
            ImageBytes = GetImageBytesFromUrl(imageUrl);
        }
        else
        {
            ImageBytes = null;
        }
        if (imageSize != 0) ImageSize = imageSize;
        else if (ImageBytes != null) ImageSize = (uint)ImageBytes.Length;
        else ImageSize = 0;
    }

    public ImageMessage(byte[]? imageBytes = null, float PicWidth = 0, float PicHeight = 0, uint imageSize = 0) : base("image")
    {
        PictureSize = new Vector2(PicWidth, PicHeight);
        FilePath = null;
        ImageUrl = null;
        ImageBytes = imageBytes;
        if (imageSize != 0) ImageSize = imageSize;
        else if (ImageBytes != null) ImageSize = (uint)ImageBytes.Length;
        else ImageSize = 0;
    }

    static private byte[]? GetImageBytesFromUrl(string imageUrl)
    {
        try
        {
            // 使用 WebClient 下载图像数据
            WebClient webClient = new WebClient();
            byte[] imageBytes = webClient.DownloadData(imageUrl);

            // 返回图像字节数组
            return imageBytes;
        }
        catch (Exception ex)
        {
            // 发生异常时返回 null
            Console.WriteLine("Error while loading image: " + ex.Message);
            return null;
        }
    }

    static private byte[]? GetImageBytesFromFilePath(string filePath)
    {
        try
        {
            // 使用 FileStream 打开文件
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                // 创建一个内存流
                MemoryStream memoryStream = new MemoryStream();

                // 将文件流复制到内存流中
                fileStream.CopyTo(memoryStream);

                // 返回内存流中的字节数组
                return memoryStream.ToArray();
            }
        }
        catch (Exception ex)
        {
            // 发生异常时返回 null
            Console.WriteLine("Error while loading image: " + ex.Message);
            return null;
        }
    }

    public override string ToPreviewText() => string.IsNullOrEmpty(ImageUrl) ? FilePath : ImageUrl;
}