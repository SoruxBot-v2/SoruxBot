using SoruxBot.SDK.Model.Message.Entity;

namespace SoruxBot.SDK.QQ.Entity;

public class ImageMessage : CommonMessage
{
    public Stream ImageStream { get; }
    
    public ImageMessage(string filePath) : base("image", new ()
    {
        {"_imageStream", new FileStream(filePath, FileMode.Open, FileAccess.Read)}
    })
    {
        ImageStream = (Stream)Content["_imageStream"];
    }

    public ImageMessage(byte[] file) : base("image", new ()
    {
        {"ImageStream", new MemoryStream(file)}
    })
    {
        ImageStream = (Stream)Content["_imageStream"];
    }
    

    public ImageMessage(Stream stream) : base("image", new ()
    {
        {"ImageStream", stream}
    })
    {
        ImageStream = stream;
    }
    
    public override string ToPreviewText() => "[图片]";
}