using SoruxBot.SDK.Model.Attribute;

namespace SoruxBot.SDK.Attribute;

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute(
    CommandPrefixType prefix,
    params string[] command)
    : System.Attribute
{
    
    /// <summary>
    /// 插件头前缀，用于区分插件的命令前缀
    /// </summary>
    public CommandPrefixType CommandPrefix { get; init; } = prefix;

    /// <summary>
    /// 插件命令数组
    /// </summary>
    public string[] Command { get; init; } = command;
}