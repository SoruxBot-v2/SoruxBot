using System.Diagnostics;

namespace SoruxBot.Kernel.Constant;

public static class OpenTelemetryHelper
{
    public static readonly ActivitySource ActivitySource = new("SoruxBot");
}