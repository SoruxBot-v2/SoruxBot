namespace SoruxBot.SDK.Attribute;

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute(
    CommandAttribute.CommandPrefixType prefix = CommandAttribute.CommandPrefixType.Global,
    bool IsLexerRequired = true,
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

    /// <summary>
    /// 插件是否需要词法分析器
    /// </summary>
    public bool IsLexerRequired { get; init; } = true;
    
    public enum CommandPrefixType
    {
        None = 0,
        Single = 1,
        Global = 2,
    }
}