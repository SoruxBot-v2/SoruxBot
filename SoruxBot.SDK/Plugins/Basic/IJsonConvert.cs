namespace SoruxBot.SDK.Plugins.Basic
{
	public interface IJsonConvert
	{
		T DeserializeObject<T>(string value);

		string GetTargetPlatform();
	}
}
