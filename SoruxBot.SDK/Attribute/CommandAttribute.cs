using SoruxBot.SDK.Model.Attribute;

namespace SoruxBot.SDK.Attribute;

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute(
    CommandPrefixType prefix,
    bool isLexerRequired,
    bool isSegmentWrapped,
    params string[] command)
    : System.Attribute
{
    /// <summary>
    /// 最原始的 CommandAttribute 构造函数，仅包含命令数组
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="command"></param>
    public CommandAttribute(
        CommandPrefixType prefix,
        params string[] command) : this(prefix, true, false, command) { }

    /// <summary>
    /// CommandAttribute 构造函数，包含命令数组和是否需要词法分析器
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="isLexerRequired"></param>
    /// <param name="command"></param>
    public CommandAttribute(
        CommandPrefixType prefix,
        bool isLexerRequired,
        params string[] command) : this(prefix, isLexerRequired, false, command) { }
    
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
    public bool IsLexerRequired { get; init; } = isLexerRequired;

    /// <summary>
    /// 插件是否需要分段包装
    /// 如果为真，那么传入的参数将以 Segment 作为颗粒度
    /// 如果为假，那么传入的参数将以字符串作为颗粒度
    /// </summary>
    public bool IsSegmentWrapped { get; init; } = isSegmentWrapped;
}