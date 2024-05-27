using Lagrange.Core;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface.Api;

namespace SoruxBot.Provider.QQ;

public class BotClient(BotConfig config, BotContext bot)
{
    public async Task LoginAsync()
    {
        Console.WriteLine("[SoruxBot.Provider.QQ] Welcome to SoruxBot.Provider.QQ.");
        Console.WriteLine("[SoruxBot.Provider.QQ] Scan qr.png to log in.");
        
        var qrCode = await bot.FetchQrCode();
        if (qrCode != null)
        {
            await File.WriteAllBytesAsync("qr.png", qrCode.Value.QrCode);
            await bot.LoginByQrCode();
            Console.WriteLine("[SoruxBot.Provider.QQ] Account has logged in.");
        }
    }
}