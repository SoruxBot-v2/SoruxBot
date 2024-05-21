namespace SoruxBot.SDK.Plugins.Basic;

/// <summary>
/// 继承本插件以确定插件的能力
/// </summary>
public class PluginController
{
    /// <summary>
    /// 当插件被启用的时候
    /// 注意：本方法可能会被在系统运行中被调用
    /// </summary>
    /// <returns></returns>
    public virtual bool OnPluginEnable()
    {
        return true;
    }
    /// <summary>
    /// 当插件被禁用的时候
    /// 注意：本方法可能会被在系统运行中被调用
    /// </summary>
    /// <returns></returns>
    public virtual bool OnPluginDisable()
    {
        return true;
    }
    /// <summary>
    /// 当插件被初始化的时候
    /// 注意：本方法应该尽量避免长时间的初始化操作，若插件加载超时，那么其会被取消加载。
    /// 数据链路：OnPluginInitialization => OnPluginEnable => OnPluginDisable
    /// </summary>
    /// <returns></returns>
    public virtual void OnPluginInitialization()
    {
        
    }
}