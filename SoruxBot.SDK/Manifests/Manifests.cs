namespace SoruxBot.SDK.Manifests;

public class Manifests
{
    /// <summary>
    /// SDK 版本号。
    /// a.b.c 中 a 为大版本号，如果不同，那么可能会出现兼容性问题
    /// b.c 仅作为小版本号更新
    /// </summary>
    public string SdkMainVersion { get; } = "0.0.1";
}