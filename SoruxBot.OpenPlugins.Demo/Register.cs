using SoruxBot.SDK.Plugins.Ability;
using SoruxBot.SDK.Plugins.Basic;

namespace SoruxBot.OpenPlugins.Demo;

public class Register : SoruxBotPlugin, ICommandPrefix
{
    public override string GetPluginName() => "DemoPlugin";

    public override string GetPluginVersion() => "1.0.0";

    public override string GetPluginAuthorName() => "OpenSoruxBot";
    public override string GetPluginDescription() => "Demo Plugin for demo";
    public string GetPluginPrefix() => "#";
}