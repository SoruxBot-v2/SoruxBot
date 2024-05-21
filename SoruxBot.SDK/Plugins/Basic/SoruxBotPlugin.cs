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
    /// <summary>
    /// 插件优先级的确定，如果当插件遇到优先级相同的插件时，那么就会根据插件的优先级来决定优先级顺序。
    /// 如果为 True，那么插件的优先级会被提高，否则插件的优先级会被降低。
    /// </summary>
    /// <returns></returns>
    public virtual bool IsUpperWhenPrivilege() => false;
}