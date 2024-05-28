using System.Collections.Concurrent;
using System.Numerics;
using System.Threading.Channels;
using Microsoft.Extensions.Configuration;
using SoruxBot.Kernel.Interface;

namespace SoruxBot.Kernel.MessageQueue;

public class ChannelPool<T> : IChannelPool<T>
{
    // Pair

    private class ChannelPair(string? bindId, Channel<T> channel)
    {
        public string? bindId { get; set; } = bindId;
        public Channel<T> channel { get; set; } = channel;
    }

    private readonly int _channelSize;
    // private readonly Dictionary<string, int> _idToChannelMap;

    // TODO: 这里的Channel是全局的，需要改成每个ChannelPool一个
    private Channel<T> _channel = Channel.CreateUnbounded<T>();

    // 内置Channel数组，是否可写、Channel
    private readonly List<ChannelPair> _channelVector;

    public ChannelPool(IConfiguration configuration)
    {
        _channelSize = configuration.GetRequiredSection("channel").GetValue<int>("size");
        _channelVector = new List<ChannelPair>(Enumerable.Repeat(
                new ChannelPair(null, Channel.CreateUnbounded<T>()), _channelSize)
            .ToArray());
        // _idToChannelMap = new Dictionary<string, int>();
    }


    private bool TryBindAvailableChannel(string bindId, out Channel<T>? channel)
    {
        // // TODO: 这里的Channel是全局的，需要改成每个ChannelPool一个
        // channel = _channel;
        // return true;

        channel = null;


        lock (this)
        {
            for (var i = 0; i < _channelSize; i++)
            {
                // 如果 id 已经绑定了，失败
                if (_channelVector[i].bindId == bindId) return false;
            }

            for (var i = 0; i < _channelSize; i++)
            {
                if (_channelVector[i].bindId != null) continue;

                // 找到未被使用的channel并使用
                _channelVector[i].bindId = bindId;
                channel = _channelVector[i].channel;
                return true;
            }
        }

        // 没有空闲的channel
        return false;
    }

    public Channel<T> RentChannel(string bindId)
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
        lock (this)
        {
            for (var i = 0; i < _channelSize; i++)
            {
                if (_channelVector[i].bindId != bindId) continue;
                channel = _channelVector[i].channel;
                return true;
            }

            return false;
        }
    }

    public bool ReturnChannel(string bindId)
    {
        lock (this)
        {
            for (var i = 0; i < _channelSize; i++)
            {
                if (_channelVector[i].bindId != bindId) continue;
                _channelVector[i].bindId = null;
                
                // 清空Reader
                while (_channelVector[i].channel.Reader.TryRead(out _))
                {
                    // 正常情况不会进入
                }

                return true;
            }

            return false;
        }
    }
}