namespace SoruxBot.SDK.Attribute;

[AttributeUsage(AttributeTargets.Method)]
public class PlatformConstraintAttribute(string platform, string action = "") : System.Attribute
{
    public string Platform { get; init; } = platform;
    
    public string Action { get; init; } = action;
}