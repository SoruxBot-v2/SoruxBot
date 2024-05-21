using Microsoft.Extensions.Configuration;
using SoruxBot.SDK.Model.Message;

namespace SoruxBot.Kernel.Bot
{
    public interface IBot
    {
        BotContext Context { get; init; }
    }
}