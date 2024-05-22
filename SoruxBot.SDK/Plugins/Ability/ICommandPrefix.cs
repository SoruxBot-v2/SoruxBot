namespace SoruxBot.SDK.Plugins.Ability;

public interface ICommandPrefix
{
    /// <summary>
    /// 得到插件命令前缀
    /// </summary>
    /// <returns></returns>
    string GetPluginPrefix();
}