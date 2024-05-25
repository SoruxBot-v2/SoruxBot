namespace SoruxBot.SDK.Model.Message.Entity;

public class TextMessage(string content) : CommonMessage("text", new ()
{
    { "content", content }
})

{
    public override string ToPreviewText()
    {
        return Content["content"].ToString()!;
    }
}