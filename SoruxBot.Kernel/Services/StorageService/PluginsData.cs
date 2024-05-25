

namespace SoruxBot.Kernel.Services.StorageService
{
	internal class PluginsData
	{
		public string PluginMark { get; set; }
		public string Key { get; set; }
		public string StringValue { get; set; }
		public PluginsData(string pluginMark, string key, string stringValue)
		{
			PluginMark = pluginMark;
			Key = key;
			StringValue = stringValue;
		}
	}
}
