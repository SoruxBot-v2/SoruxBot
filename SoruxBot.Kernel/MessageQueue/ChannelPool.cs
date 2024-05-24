using System.Numerics;
using System.Threading.Channels;

namespace SoruxBot.Kernel.MessageQueue;

public class ChannelPool<T>
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
   

    private Channel<T>? TryBindAvailableChannel(string id)
    {
        for (var i = 0; i < _channelSize; i++)
        {
            lock (_idToChannelMap)
            {
                if (_idToChannelMap.ContainsValue(i)) continue;
                _idToChannelMap[id] = i;
                return _channelVector[i];
            }
        }

        return null;
    }

    public Channel<T> GetChannel(string contextId)
    {
        if (_idToChannelMap.TryGetValue(contextId, out var value))
        {
            return _channelVector[value];
        }

        return TryBindAvailableChannel(contextId) ?? throw new Exception("No available channel");
    }

    public bool ReturnChannel(string contextId)
    {
        if (!_idToChannelMap.Remove(contextId, out var value))
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