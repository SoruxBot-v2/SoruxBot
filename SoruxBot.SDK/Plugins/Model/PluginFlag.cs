namespace SoruxBot.SDK.Plugins.Model;

public enum PluginFlag
{
    /// <summary>
    /// 消息处理，表示消息已经被处理了
    /// </summary>
    MsgPassed = 0,
    /// <summary>
    /// 消息拦截，表示消息不会被继续传递
    /// </summary>
    MsgIntercepted = 1,
    /// <summary>
    /// 消息忽略，表示消息被忽略处理了
    /// </summary>
    MsgIgnored = 2,
    /// <summary>
    /// 消息未经过处理
    /// </summary>
    MsgUnprocessed = 3
}