using SoruxBot.SDK.Plugins.Model;

namespace SoruxBot.SDK.Plugins.Basic;

public abstract class SoruxBotLib
{
    /// <summary>
    /// 得到库的名称
    /// </summary>
    public abstract string GetLibName();
    /// <summary>
    /// 得到库的版本
    /// </summary>
    public abstract string GetLibVersion();
    /// <summary>
    /// 得到库作者的名称
    /// </summary>
    public abstract string GetLibAuthorName();
    /// <summary>
    /// 得到库的描述方式
    /// </summary>
    public abstract string GetLibDescription();
    /// <summary>
    /// 得到类库的类型
    /// </summary>
    /// <returns></returns>
    public abstract LibType GetLibType();
}