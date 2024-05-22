namespace SoruxBot.SDK.QQ.Entity;

public class FileEntity
{
    public long FileSize { get; internal set; }

    public string FileName { get; internal set; }

    public byte[] FileMd5 { get; internal set; }

    public string? FileUrl { get; internal set; }

    /// <summary>
    /// Only Group File has such field
    /// </summary>
    public string? FileId { get; set; }

    public FileEntity()
    {
        FileName = "";
        FileMd5 = Array.Empty<byte>();
        FileSha1 = Array.Empty<byte>();
    }

    /// <summary>
    /// This entity could not be directly sent via <see cref="MessageChain"/>,
    /// it should be sent via <see cref="Lagrange.Core.Common.Interface.Api.GroupExt.GroupFSUpload"/>
    /// </summary>
    public FileEntity(string path)
    {
        FileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        FileMd5 = FileStream.Md5().UnHex();
        FileSize = FileStream.Length;
        FileName = Path.GetFileName(path);
        FileSha1 = FileStream.Sha1().UnHex();
    }

    /// <summary>
    /// This entity could not be directly sent via <see cref="MessageChain"/>,
    /// it should be sent via <see cref="Lagrange.Core.Common.Interface.Api.GroupExt.GroupFSUpload"/>
    /// </summary>
    public FileEntity(byte[] payload, string fileName)
    {
        FileStream = new MemoryStream(payload);
        FileMd5 = payload.Md5().UnHex();
        FileSize = payload.Length;
        FileName = fileName;
        FileSha1 = FileStream.Sha1().UnHex();
    }

    public string ToPreviewString() => $"[File] {FileName} ({FileSize}): {FileUrl ?? "failed to receive file url"}";

    public string ToPreviewText() => $"[文件] {FileName}";
}