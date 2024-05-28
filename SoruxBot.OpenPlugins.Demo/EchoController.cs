using System.Diagnostics.Tracing;
using SoruxBot.SDK.Attribute;
using SoruxBot.SDK.Model.Attribute;
using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Plugins.Basic;
using SoruxBot.SDK.Plugins.Model;
using SoruxBot.SDK.Plugins.Service;

namespace SoruxBot.OpenPlugins.Demo;

public class EchoController(ILoggerService loggerService, ICommonApi bot) : PluginController
{

    [MessageEvent(MessageType.PrivateMessage)]
    [Command(CommandPrefixType.Single, "echo [content]")]
    [PlatformConstraint("QQ", "FriendMessage")]
    public PluginFlag EchoPlugin(MessageContext ctx, string content)
    {
        var chain = MessageBuilder
            .PrivateMessage(ctx.TriggerId, "qq")
            .Text("hello")
            .Build();
        ctx.MessageChain = chain;
        bot.SendMessage(ctx);
        return PluginFlag.MsgPassed;
    }
}