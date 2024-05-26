

namespace SoruxBot.Kernel.Services.StorageService
{
	internal class PluginsData(string pluginMark, string key, string stringValue)
	{
		public string PluginMark { get; set; } = pluginMark;
		public string Key { get; set; } = key;
		public string StringValue { get; set; } = stringValue;
	}
}
