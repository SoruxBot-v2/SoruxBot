namespace SoruxBot.SDK.Plugins.Model;

/// <summary>
/// 类库的帮助信息
/// </summary>
public enum LibType
{
    /// <summary>
    /// 用户类库，表示由用户开发的类库。
    /// 该类类库会在插件启动之前，被扫描类库下的所有接口定义文件，然后传入 SoruxBot Interface Libs
    /// 若插件的构造函数期待某一个特定的 Interface，那么会被传入调用
    /// </summary>
    UserLib = 1,
    /// <summary>
    /// SDK 类库，表示该 类库是直接为 SDK 提供帮助支持的。
    /// 我们认为规定，SDK 类库与 SDK 相绑定，由 SDK 类库提供 SDK 特定平台的工作。
    /// 如 JsonConverter
    /// </summary>
    SdkLib = 2,
}