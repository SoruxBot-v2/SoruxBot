namespace SoruxBot.SDK.Plugins.Basic;

/// <summary>
/// 继承本类以确定你的插件描述信息
/// </summary>
public abstract class SoruxBotPlugin
{
    /// <summary>
    /// 得到插件的名称
    /// </summary>
    public abstract string GetPluginName();
    /// <summary>
    /// 得到插件的版本
    /// </summary>
    public abstract string GetPluginVersion();
    /// <summary>
    /// 得到插件作者的名称
    /// </summary>
    public abstract string GetPluginAuthorName();
    /// <summary>
    /// 得到插件的描述方式
    /// </summary>
    public abstract string GetPluginDescription();
}