using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Lagrange.Core.Utility.Sign;

namespace SoruxBot.Provider.QQ
{
    internal class QqSigner: SignProvider
    {
        private readonly HttpClient _client = new();

        private const string WindowsUrl = "https://sign.lagrangecore.org/api/sign";

        public override byte[]? Sign(string cmd, uint seq, byte[] body, out byte[]? ver, out string? token)
        {
            ver = null;
            token = null;
            if (!WhiteListCommand.Contains(cmd)) return null;
            if (!Available || string.IsNullOrEmpty(WindowsUrl)) return new byte[35]; // Dummy signature

            var payload = new JsonObject
        {
            { "cmd", cmd },
            { "seq", seq },
            { "src", Hex(body) },
        };

            try
            {
                var message = _client.PostAsJsonAsync(WindowsUrl, payload).Result;
                string response = message.Content.ReadAsStringAsync().Result;
                var json = JsonSerializer.Deserialize<JsonObject>(response);

                ver = UnHex(json?["value"]?["extra"]?.ToString()) ?? Array.Empty<byte>();
                token = Encoding.ASCII.GetString(UnHex(json?["value"]?["token"]?.ToString()) ?? Array.Empty<byte>());
                return UnHex(json?["value"]?["sign"]?.ToString()) ?? new byte[35];
            }
            catch (Exception)
            {
                Available = false;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{nameof(QqSigner)}] Failed to get signature, using dummy signature");

                return new byte[35]; // Dummy signature
            }
        }

        public override bool Test()
        {
            throw new NotImplementedException();
        }

        private static string Hex(byte[] bytes, bool lower = false, bool space = false)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString(lower ? "x2" : "X2"));
                if (space) sb.Append(' ');
            }
            return sb.ToString();
        }

        private static byte[] UnHex(string hex)
        {
            if (hex.Length % 2 != 0) throw new ArgumentException("Invalid hex string");

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2) bytes[i / 2] = byte.Parse(hex.Substring(i, 2), NumberStyles.HexNumber);
            return bytes;
        }
    }


}
