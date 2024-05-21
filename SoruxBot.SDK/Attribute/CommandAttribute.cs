namespace SoruxBot.SDK.Attribute;

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute(
    CommandAttribute.CommandPrefixType prefix = CommandAttribute.CommandPrefixType.Global,
    params string[] command)
    : System.Attribute
{
    public CommandPrefixType CommandPrefix { get; init; } = prefix;
    public string[] Command { get; init; } = command;

    public enum CommandPrefixType
    {
        None = 0,
        Single = 1,
        Global = 2,
    }
}