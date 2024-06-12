using SoruxBot.SDK.Model.Message.Entity;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;

namespace SoruxBot.SDK.QQ.Entity;

public class RecordMessage : CommonMessage

{
    /// <summary>
    /// 音频时长
    /// </summary>
    public int AudioLength { get; set; }

    /// <summary>
    /// 音频本地路径
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// 音频名称
    /// </summary>
    public string AudioName { get; set; }

    /// <summary>
    /// 音频字节大小
    /// </summary>
    public int AudioSize { get; set; }

    /// <summary>
    /// 音频Url
    /// </summary>
    public string AudioUrl { get; set; }

    /// <summary>
    /// 音频字节数据
    /// </summary>
    public byte[] AudioBytes { get; set; }

    public RecordMessage(string audioUrl = null, string filePath = null, byte[] audioBytes = null, int audioLength = 0, string audioName = null, int audioSize = 0) : base("record")
    {
        FilePath = filePath;
        AudioUrl = audioUrl;
        AudioLength = audioLength;
        AudioName = audioName;
        if (audioBytes != null) audioBytes = audioBytes;
        else if (filePath != null)
        {
            AudioBytes = GetAudioBytesFromFilePath(filePath);
        }
        else if (audioUrl != null)
        {
            AudioBytes = GetAudioBytesFromUrl(audioUrl);
        }
        else
        {
            AudioBytes = null;
        }
        if (audioSize != 0) AudioSize = AudioSize;
        else if (AudioBytes != null) AudioSize = (int)AudioBytes.Length;
        else AudioSize = 0;
    }

    public RecordMessage(byte[] audioBytes = null, int audioLength = 0, string audioName = null, int audioSize = 0) : base("record")
    {
        AudioLength = audioLength;
        AudioName = audioName;
        FilePath = null;
        AudioUrl = null;
        AudioBytes = audioBytes;
        if (audioSize != 0) AudioSize = AudioSize;
        else if (AudioBytes != null) AudioSize = (int)AudioBytes.Length;
        else AudioSize = 0;
    }

    static private byte[] GetAudioBytesFromUrl(string recordUrl)
    {
        try
        {
            // 使用 WebClient 下载音频数据
            WebClient webClient = new WebClient();
            byte[] recordBytes = webClient.DownloadData(recordUrl);

            // 返回图像字节数组
            return recordBytes;
        }
        catch (Exception ex)
        {
            // 发生异常时返回 null
            Console.WriteLine("Error while loading record: " + ex.Message);
            return null;
        }
    }

    static private byte[] GetAudioBytesFromFilePath(string filePath)
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
            Console.WriteLine("Error while loading record: " + ex.Message);
            return null;
        }
    }

    public override string ToPreviewText() => string.IsNullOrEmpty(AudioUrl) ? FilePath : AudioUrl;
}