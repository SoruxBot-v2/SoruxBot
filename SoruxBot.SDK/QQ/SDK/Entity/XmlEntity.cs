namespace SoruxBot.SDK.QQ.SDK.Entity;

public class XmlEntity 
{
    public string Xml { get; set; }

    public XmlEntity() => Xml = "";

    public XmlEntity(string xml) => Xml = xml;

    public string ToPreviewString() => $"[Xml]: {Xml}";
}