namespace SoruxBot.Kernel.Services.PluginService.Model;

/// <summary>
/// 插件描述类别，用于描述插件 Action 的具体信息
/// </summary>
public class PluginsActionDescriptor(Delegate actionDelegate, string actionName, string pluginName, string instanceTypeName)
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
    /// 插件动作名称
    /// </summary>
    public string ActionName { get; set; } = actionName;
    
    /// <summary>
    /// 插件名称
    /// </summary>
    public string PluginName { get; set; } = pluginName;

    /// <summary>
    /// 插件实例的类型名称
    /// </summary>
    public string InstanceTypeName { get; set; } = instanceTypeName;
}