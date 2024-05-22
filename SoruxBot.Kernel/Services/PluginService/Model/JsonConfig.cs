namespace SoruxBot.Kernel.Services.PluginService.Model
{
    public class PluginJsonConfig(string name, int privilege)
    {
        /// <summary>
        /// 插件名称
        /// </summary>
        public string Name { get; init; } = name;

        /// <summary>
        /// 插件权限等级
        /// </summary>
        public int Privilege  { get; init; } = privilege;
    }
}