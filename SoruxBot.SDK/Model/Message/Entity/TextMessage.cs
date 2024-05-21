namespace SoruxBot.SDK.Model.Message.Entity;

public class TextMessage(string type, string content) : CommonMessage(type, content)
{
    public override string ToPreviewText()
    {
        return Content;
    }
}