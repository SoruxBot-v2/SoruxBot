using System.Collections.Concurrent;
using System.Numerics;
using System.Threading.Channels;
using SoruxBot.Kernel.Interface;

namespace SoruxBot.Kernel.MessageQueue;

public class ChannelPool<T> : IChannelPool<T>
{
    private readonly int _channelSize;
    private readonly Dictionary<string, int> _idToChannelMap;

    // 内置Channel数组
    private readonly Vector<Channel<T>> _channelVector;

    public ChannelPool(int size)
    {
        _channelSize = size;
        _channelVector = new Vector<Channel<T>>(Enumerable.Repeat(
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

    public Channel<T> GetBindChannel(string bindId)
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