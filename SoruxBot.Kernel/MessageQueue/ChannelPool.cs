using System.Collections.Concurrent;
using System.Numerics;
using System.Threading.Channels;
using Microsoft.Extensions.Configuration;
using SoruxBot.Kernel.Interface;

namespace SoruxBot.Kernel.MessageQueue;

public class ChannelPool<T> : IChannelPool<T>
{
    private readonly int _channelSize;
    private readonly Dictionary<string, int> _idToChannelMap;

    // 内置Channel数组
    private readonly List<Channel<T>> _channelVector;

    public ChannelPool(IConfiguration configuration)
    {
        _channelSize = configuration.GetRequiredSection("channel").GetValue<int>("size");
        _channelVector = new List<Channel<T>>(Enumerable.Repeat(
                Channel.CreateUnbounded<T>(), _channelSize)
            .ToArray());
        _idToChannelMap = new Dictionary<string, int>();
    }


    private bool TryBindAvailableChannel(string bindId, out Channel<T>? channel)
    {
        channel = null;
        for (var i = 0; i < _channelSize; i++)
        {
            lock (_idToChannelMap)
            {
                // 如果 id 已经绑定了，失败
                if (_idToChannelMap.ContainsKey(bindId)) return false;

                // 如果Channel被其他id绑定了，跳过
                if (_idToChannelMap.ContainsValue(i)) continue;

                // 将id绑定到当前Channel
                _idToChannelMap[bindId] = i;
                channel = _channelVector[i];
                return true;
            }
        }

        return false;
    }

    public Channel<T> CreateBindChannel(string bindId)
    {
        Channel<T>? channel;
        // 直到绑定成功
        while (!TryBindAvailableChannel(bindId, out channel))
        {
            Thread.Sleep(0);
        }

        // 这里的 channel 一定不为空
        return channel!;
    }

    public bool TryGetBindChannel(string bindId, out Channel<T>? channel)
    {
        channel = null;
        if (_idToChannelMap.TryGetValue(bindId, out var value)) return false;
        channel = _channelVector[value];
        return true;

    }

    public bool ReturnChannel(string bindId)
    {
        if (!_idToChannelMap.Remove(bindId, out var value))
        {
            return false;
        }

        // 清空Reader
        while (_channelVector[value].Reader.TryRead(out _))
        {
        }

        return true;
    }
}