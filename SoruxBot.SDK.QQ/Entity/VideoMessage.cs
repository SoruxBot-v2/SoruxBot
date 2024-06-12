using SoruxBot.SDK.Model.Message.Entity;
using System.Net;
using System.Numerics;

namespace SoruxBot.SDK.QQ.Entity;

public class VideoMessage : CommonMessage

{
    /// <summary>
    /// 视频本地路径
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// 视频文件的哈希值
    /// </summary>
    public string VideoHash { get; set; }

    /// <summary>
    /// 视频大小（长宽）
    /// </summary>
    public Vector2 Size { get; }

    /// <summary>
    /// 视频字节大小
    /// </summary>
    public int VideoSize { get; set; }

    /// <summary>
    /// 视频Url
    /// </summary>
    public string VideoUrl { get; set; }

    /// <summary>
    /// 视频字节数据
    /// </summary>
    public byte[] VideoBytes { get; set; }

    public VideoMessage(string videoUrl = null, string filePath = null, byte[] videoBytes = null, string videoHash = null, float picWidth = 0, float picHeight = 0, int videoSize = 0) : base("video")
    {
        Size = new Vector2(picWidth, picHeight);
        FilePath = filePath;
        VideoUrl = videoUrl;
        VideoHash = videoHash;
        if (videoBytes != null) VideoBytes = videoBytes;
        else if (filePath != null)
        {
            VideoBytes = GetVideoBytesFromFilePath(filePath);
        }
        else if (videoUrl != null)
        {
            VideoBytes = GetVideoBytesFromUrl(videoUrl);
        }
        else
        {
            VideoBytes = null;
        }
        if (videoSize != 0) VideoSize = videoSize;
        else if (VideoBytes != null) VideoSize = (int)VideoBytes.Length;
        else VideoSize = 0;
    }

    public VideoMessage(byte[] imageBytes = null, string videoHash = null, float picWidth = 0, float picHeight = 0, int videoSize = 0) : base("video")
    {
        Size = new Vector2(picWidth, picHeight);
        FilePath = null;
        VideoUrl = null;
        VideoHash = videoHash;
        VideoBytes = imageBytes;
        if (videoSize != 0) VideoSize = videoSize;
        else if (VideoBytes != null) VideoSize = (int)VideoBytes.Length;
        else VideoSize = 0;
    }

    static private byte[] GetVideoBytesFromUrl(string videoUrl)
    {
        try
        {
            // 使用 WebClient 下载视频数据
            WebClient webClient = new WebClient();
            byte[] videoBytes = webClient.DownloadData(videoUrl);

            // 返回图像字节数组
            return videoBytes;
        }
        catch (Exception ex)
        {
            // 发生异常时返回 null
            Console.WriteLine("Error while loading video: " + ex.Message);
            return null;
        }
    }

    static private byte[] GetVideoBytesFromFilePath(string filePath)
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
            Console.WriteLine("Error while loading video: " + ex.Message);
            return null;
        }
    }

    public override string ToPreviewText() => string.IsNullOrEmpty(VideoUrl) ? FilePath : VideoUrl;
}