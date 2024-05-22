namespace SoruxBot.SDK.QQ.SDK.Entity;

public class KeyboardEntity
{
    public KeyboardData Data { get; set; }

    public KeyboardEntity(KeyboardData data) => Data = data;

    public KeyboardEntity(string data) => Data = JsonSerializer.Deserialize<KeyboardData>(data) ?? throw new Exception();

    public string ToPreviewString() => throw new NotImplementedException();
}