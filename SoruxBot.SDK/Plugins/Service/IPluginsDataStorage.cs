namespace SoruxBot.SDK.Plugins.Service
{
    public interface IPluginsDataStorage
    {
        // 字符串工具
        bool AddStringSettings(string pluginMark, string key, string value);
        bool RemoveStringSettings(string pluginMark, string key);
        string GetStringSettings(string pluginMark, string key);
        bool EditStringSettings(string pluginMark, string key, string value);
        // 二进制工具
        bool AddBinarySettings(string pluginMark, string key, byte[] value);
        bool RemoveBinarySettings(string pluginMark, string key);
        byte[]? GetBinarySettings(string pluginMark, string key);
        bool EditBinarySettings(string pluginMark, string key, byte[] value);
    }
}