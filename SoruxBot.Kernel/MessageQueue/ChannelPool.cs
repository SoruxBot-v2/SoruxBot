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


    private Channel<T>? TryBindAvailableChannel(string bindId)
    {
        for (var i = 0; i < _channelSize; i++)
        {
            lock (_idToChannelMap)
            {
                if (_idToChannelMap.ContainsValue(i)) continue;
                _idToChannelMap[bindId] = i;
                return _channelVector[i];
            }
        }

        return null;
    }

    public Channel<T> GetBindChannel(string bindId)
    {
        if (_idToChannelMap.TryGetValue(bindId, out var value))
        {
            return _channelVector[value];
        }

        return TryBindAvailableChannel(bindId) ?? throw new Exception("No available channel");
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