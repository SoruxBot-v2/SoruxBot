using SoruxBot.SDK.Plugins.Basic;
namespace SoruxBot.Kernel.Services.PluginService.JsonConvertService
{
	public class JsonConvertMap
	{
		private Dictionary<string, IJsonConvert> _jsonConvertMap = new();
		public void Add(string key, IJsonConvert jsonConvert)
		{
			_jsonConvertMap.Add(key, jsonConvert);
		}
		public void Remove(string key)
		{
			_jsonConvertMap.Remove(key);
		}
		public bool TryGet(string key, out IJsonConvert? jsonConvert)
		{
			return _jsonConvertMap.TryGetValue(key, out jsonConvert);
		}
	}
}
