namespace SoruxBot.Kernel.Services.PluginService.Model;

/// <summary>
/// 插件描述类别，用于描述插件 Action 的具体信息
/// </summary>
public class PluginsActionDescriptor(Delegate actionDelegate, string pluginName, string instanceTypeName, bool isParameterLexerDisable = false)
{
    /// <summary>
    /// Action对应的参数
    /// </summary>
    public List<PluginActionParameter> ActionParameters { get; set; } = new ();

    /// <summary>
    /// 插件委托
    /// </summary>
    public Delegate ActionDelegate { get; set; } = actionDelegate;

    /// <summary>
    /// 插件名称
    /// </summary>
    public string PluginName { get; set; } = pluginName;

    /// <summary>
    /// 插件实例的类型名称
    /// </summary>
    public string InstanceTypeName { get; set; } = instanceTypeName;

    /// <summary>
    /// 如果为 false，那么不进行 Lexer 绑定，直接输入到第一个 Parameter 里面，以字符串的形式
    /// </summary>
    public bool IsParameterLexerDisable { get; set; } = false;
}