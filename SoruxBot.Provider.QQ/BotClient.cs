using Lagrange.Core;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Interface.Api;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace SoruxBot.Provider.QQ;

public class BotClient(bool isFastLogin)
{
    public BotContext bot { get; set; }
    public bool IsFastLogin { get; set; } = isFastLogin;
    public async Task LoginAsync()
    {
        if (IsFastLogin)
        {
            Console.WriteLine("[SoruxBot.Provider.QQ] Welcome to SoruxBot.Provider.QQ.");
            var deviceInfo = GetDeviceInfo();
            var keyStore = LoadKeystore();
            bot = BotFactory.Create(new BotConfig()
            {
                UseIPv6Network = false,
                GetOptimumServer = true,
                AutoReconnect = true,
                Protocol = Protocols.Linux,
                CustomSignProvider = new QqSigner(),
            }, deviceInfo, keyStore);

            if (keyStore == null)
            {
                keyStore = new BotKeystore();
                Console.WriteLine("Please login by QrCode first");
                Console.WriteLine("[SoruxBot.Provider.QQ] Scan qr.png to log in.");
                bot = BotFactory.Create(new BotConfig
                {
                    UseIPv6Network = false,
                    GetOptimumServer = true,
                    AutoReconnect = true,
                    Protocol = Protocols.Linux,
                    CustomSignProvider = new QqSigner(),
                }, deviceInfo, keyStore);


                bot.Invoker.OnBotOnlineEvent += (context, @event) =>
                {
                    SaveKeystore(bot.UpdateKeystore());
                };

                var qrCode = await bot.FetchQrCode();
                if (qrCode != null)
                {
                    await File.WriteAllBytesAsync("qr.png", qrCode.Value.QrCode);
                    await bot.LoginByQrCode();
                    Console.WriteLine("[SoruxBot.Provider.QQ] Account has logged in.");
                }
                return;
            }

            bot.Invoker.OnBotOnlineEvent += (_, @event) =>
            {
                SaveKeystore(bot.UpdateKeystore());
            };


            await bot.LoginByPassword();
            Console.WriteLine("[SoruxBot.Provider.QQ] Account has logged in.");
        }

        else
        {
            Console.WriteLine("[SoruxBot.Provider.QQ] Welcome to SoruxBot.Provider.QQ.");
            Console.WriteLine("[SoruxBot.Provider.QQ] Scan qr.png to log in.");

            var deviceInfo = GetDeviceInfo();
            var keyStore = LoadKeystore() ?? new BotKeystore();
            bot = BotFactory.Create(new BotConfig()
            {
                UseIPv6Network = false,
                GetOptimumServer = true,
                AutoReconnect = true,
                Protocol = Protocols.Linux,
                CustomSignProvider = new QqSigner(),
            }, deviceInfo, keyStore);
            var qrCode = await bot.FetchQrCode();
            if (qrCode != null)
            {
                await File.WriteAllBytesAsync("qr.png", qrCode.Value.QrCode);
                await bot.LoginByQrCode();
                Console.WriteLine("[SoruxBot.Provider.QQ] Account has logged in.");
            }
        }
    }

    public static BotDeviceInfo GetDeviceInfo()
    {
        if (File.Exists("DeviceInfo.json"))
        {
            var info = JsonSerializer.Deserialize<BotDeviceInfo>(File.ReadAllText("DeviceInfo.json"));
            if (info != null) return info;

            info = new()
            {
                Guid = Guid.NewGuid(),
                MacAddress = GenRandomBytes(6),
                DeviceName = $"SoruxBot-QQ",
                SystemKernel = "Ubuntu 20.04.3 LTS",
                KernelVersion = "5.4.0-81-generic"
            };
            File.WriteAllText("DeviceInfo.json", JsonSerializer.Serialize(info));
            return info;
        }

        BotDeviceInfo deviceInfo = new()
        {
            Guid = Guid.NewGuid(),
            MacAddress = GenRandomBytes(6),
            DeviceName = $"SoruxBot-QQ",
            SystemKernel = "Ubuntu 20.04.3 LTS",
            KernelVersion = "5.4.0-81-generic"
        };
        File.WriteAllText("DeviceInfo.json", JsonSerializer.Serialize(deviceInfo));
        return deviceInfo;
    }

    public static void SaveKeystore(BotKeystore keystore) =>
        File.WriteAllText("Keystore.json", JsonSerializer.Serialize(keystore));

    public static BotKeystore? LoadKeystore()
    {
        try
        {
            var text = File.ReadAllText("Keystore.json");
            return JsonSerializer.Deserialize<BotKeystore>(text, new JsonSerializerOptions()
            {
                ReferenceHandler = ReferenceHandler.Preserve
            });
        }
        catch
        {
            return null;
        }
    }

    static byte[] GenRandomBytes(int length)
    {
        byte[] randomBytes = new byte[length];
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return randomBytes;
    }
}

