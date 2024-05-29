using System.Collections.Concurrent;
using System.Numerics;
using System.Threading.Channels;
using Microsoft.Extensions.Configuration;
using SoruxBot.Kernel.Interface;

namespace SoruxBot.Kernel.MessageQueue;

public class ChannelPool<TI, TO> : IChannelPool<TI, TO>
{
    // Pair
    private class ChannelEntity : IChannelPairEntity<TI, TO>
    {
        // channel in and out
        private Tuple<Channel<TI>, Channel<TO>> channelPair { get; }

        // 元信息
        private string? _bindId;
        private long? _rentTime;

        private bool _isRunning = true;
        private readonly Task _worker;

        private readonly ConcurrentQueue<Func<TI, TO>> _workActions;
        private readonly SemaphoreSlim _semaphore;

        public Tuple<Channel<TI>, Channel<TO>> GetChannelPair()
        {
            return channelPair;
        }

        public void PushWork(Func<TI, TO> work)
        {
            _workActions.Enqueue(work);
            _semaphore.Release(1);
        }

        public ChannelEntity(string? id, long? time, Tuple<Channel<TI>, Channel<TO>> c)
        {
            _bindId = id;
            _rentTime = time;
            channelPair = c;
            _workActions = new ConcurrentQueue<Func<TI, TO>>();
            _semaphore = new SemaphoreSlim(0);

            // worker
            _worker = new Task<Task>(async () =>
            {
                while (_isRunning)
                {
                    var inData = await c.Item1.Reader.ReadAsync();
                    _semaphore.Wait();

                    // 有任务，以输入chanel的数据为输入，执行任务，将结果写入输出channel
                    if (!_workActions.TryDequeue(out var action)) continue;
                    var outData = action(inData);
                    await c.Item2.Writer.WriteAsync(outData);
                }
            });
            _worker.Start();
        }

        ~ChannelEntity()
        {
            _isRunning = false;
            _worker.Dispose();
        }

        public string? GetBindId()
        {
            return _bindId;
        }

        public long? GetRentTime()
        {
            return _rentTime;
        }

        public void SetMeta(string? id, long? time)
        {
            _bindId = id;
            _rentTime = time;
        }
    }

    private readonly int _channelSize;
    private readonly SemaphoreSlim _channelSemaphore;

    // 内置Channel数组
    private readonly List<ChannelEntity> _channelVector;

    public ChannelPool(IConfiguration configuration)
    {
        _channelSize = configuration.GetRequiredSection("channel").GetValue<int>("size");
        _channelSemaphore = new SemaphoreSlim(_channelSize);


        _channelVector = new List<ChannelEntity>(Enumerable.Repeat(
            new ChannelEntity(null, null,
                new Tuple<Channel<TI>, Channel<TO>>(
                    Channel.CreateUnbounded<TI>(), Channel.CreateUnbounded<TO>())
            ),
            _channelSize)
        );
    }

    private bool IfOutOfDate(int i)
    {
        // 如果已经超时（借用超过10秒，也就是两次获取这个id的channel间隔超过10s）,返回true
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _channelVector[i].GetRentTime() > 10000;
    }

    private bool TryGetBindChannelPair(string bindId, out IChannelPairEntity<TI,TO>? channelPair)
    {
        channelPair = null;

        lock (this)
        {
            for (var i = 0; i < _channelSize; i++)
            {
                if (_channelVector[i].GetBindId() != bindId) continue;

                // 如果已经绑定，检查是否超时, (既然绑定，一定可以拿到超时时间)

                if (IfOutOfDate(i))
                {
                    // 归还channel
                    ReturnChannel(i);
                    // 重新去借
                    break;
                }

                // 未超时，续签借用时间，返回channel
                _channelVector[i].SetMeta(bindId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                channelPair = _channelVector[i];
                return true;
            }

            // 没有找到绑定的channel,或者绑定的channel已过期
            for (var i = 0; i < _channelSize; i++)
            {
                // 这个channel没有被使用且未过期
                if (_channelVector[i].GetBindId() != null)
                {
                    if (!IfOutOfDate(i))
                    {
                        continue;
                    }

                    // 过期则归还channel
                    ReturnChannel(i);
                }

                // 找到了未被使用的channel并使用
                _channelVector[i].SetMeta(bindId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                channelPair = _channelVector[i];
                return true;
            }
        }

        // 没有空闲的channel
        return false;
    }

    // 借用 channel
    public IChannelPairEntity<TI,TO> RentChannelPair(string bindId)
    {
        // 直到绑定成功
        _channelSemaphore.Wait();

        if (!TryGetBindChannelPair(bindId, out var channelPair))
        {
            Console.WriteLine("ChannelPool: Rent channel failed");
            Environment.Exit(-10087);
        }

        // 这里的 channel 一定不为空
        return channelPair!;
    }


    private void ReturnChannel(int i)
    {
        if (i < 0 || i >= _channelSize)
        {
            Console.WriteLine("ChannelPool: Return channel failed, index out of range");
            Environment.Exit(-10088);
            return;
        }

        // 解绑，归还 channel
        _channelVector[i].SetMeta(null, null);

        while (_channelVector[i].GetChannelPair().Item1.Reader.TryRead(out _))
        {
            // 正常情况不会进入
            Console.WriteLine("ChannelPool: Channel1 is not empty, return channel failed");
            Environment.Exit(-10085);
        }

        while (_channelVector[i].GetChannelPair().Item2.Reader.TryRead(out _))
        {
            // 正常情况不会进入
            Console.WriteLine("ChannelPool: Channel2 is not empty, return channel failed");
            Environment.Exit(-10086);
        }

        _channelSemaphore.Release(1);
    }
}