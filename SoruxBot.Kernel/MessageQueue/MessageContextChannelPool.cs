using System.Collections;
using System.Numerics;
using System.Threading.Channels;
using SoruxBot.SDK.Model.Message;

namespace SoruxBot.Kernel.MessageQueue;

public static class MessageContextChannelPool
{
    private const int ChannelSize = 20;
    private static readonly Dictionary<string, int> CtxToChannelMap = new();

    // 创建一个长度为20的Channel数组
    private static readonly Vector<Channel<MessageContext>> ChannelVector =
        new(Enumerable.Repeat(Channel.CreateUnbounded<MessageContext>(), ChannelSize).ToArray());

    private static Channel<MessageContext>? TryBindAvailableChannel(string contextId)
    {
        for (var i = 0; i < ChannelSize; i++)
        {
            lock (CtxToChannelMap)
            {
                if (CtxToChannelMap.ContainsValue(i)) continue;
                CtxToChannelMap[contextId] = i;
                return ChannelVector[i];
            }
        }

        return null;
    }

    public static Channel<MessageContext> GetChannel(string contextId)
    {
        if (CtxToChannelMap.TryGetValue(contextId, out var value))
        {
            return ChannelVector[value];
        }

        return TryBindAvailableChannel(contextId) ?? throw new Exception("No available channel");
    }

    public static bool ReturnChannel(string contextId)
    {
        if (!CtxToChannelMap.Remove(contextId, out var value))
        {
            return false;
        }
        
        // 清空Reader
        while (ChannelVector[value].Reader.TryRead(out _))
        {
        }

        return true;



    }
}