using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoruxBot.SDK.Model.Message;

namespace SoruxBot.Kernel.Bot
{
    public class Bot(BotContext context) : IBot
    {
        public BotContext Context { get; init; } = context;
    }
}