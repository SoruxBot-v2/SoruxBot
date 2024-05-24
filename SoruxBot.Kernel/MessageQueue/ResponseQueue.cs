using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using SoruxBot.Kernel.Interface;
using SoruxBot.SDK.Model.Message;

namespace SoruxBot.Kernel.MessageQueue;

public class ResponseQueueImpl(
    IChannelPool<MessageContext> msgChannelPool,
    IChannelPool<string> syncChannelPool) : IResponseQueue
{
    private readonly ConcurrentDictionary<string, bool> _bindIds = new();

    // 传递要发送的消息
    private readonly IChannelPool<MessageContext> _msgChannelPool = msgChannelPool;

    // 实现方法同步
    private readonly IChannelPool<string> _syncChannel = syncChannelPool;

    public async Task<string> SetNextResponse(MessageContext context)
    {
        // 这里是这次回话的全局唯一ID
        var bindId = context.ContextId;
        _bindIds[bindId] = true;

        await _msgChannelPool.GetBindChannel(bindId).Writer.WriteAsync(context);
        var res = await _syncChannel.GetBindChannel(bindId).Reader.ReadAsync();

        // 归还同步channel
        _syncChannel.ReturnChannel(bindId);

        return res;
    }

    public bool TryGetNextResponse(Func<MessageContext, string> func)
    {
        MessageContext? context = null;
        lock (_bindIds)
        {
            foreach (var (key, _) in _bindIds)
            {
                if (!_msgChannelPool.GetBindChannel(key).Reader.TryRead(out context)) continue;

                // 归还消息channel
                _msgChannelPool.ReturnChannel(key);
                _bindIds.Remove(key, out _);
            }
        }

        // 如果没有有消息的channel，或者写入失败，返回false
        return context != null &&
               _syncChannel
                   .GetBindChannel(context.TiedId)
                   .Writer.TryWrite(func(context));
    }
}