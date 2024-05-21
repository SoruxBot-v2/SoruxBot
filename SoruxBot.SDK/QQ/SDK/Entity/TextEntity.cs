namespace SoruxBot.SDK.QQ.SDK.Entity;

public class TextEntity 
{
    public string Text { get; set; }

    public TextEntity() => Text = "";

    public TextEntity(string text) => Text = text;

    public string ToPreviewString()
    {
        return $"[Text]: {Text}";
    }

    public string ToPreviewText() => Text;
}