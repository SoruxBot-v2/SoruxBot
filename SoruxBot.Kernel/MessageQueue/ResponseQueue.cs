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

    // 实现方法同步

    public async Task<string> SetNextResponse(MessageContext context)
    {
        // 这里是发送给指定用户实体的id
        var bindId = context.TargetPlatform + context.TriggerPlatformId + context.TriggerId;
        _bindIds[bindId] = true;


        await msgChannelPool.GetBindChannel(bindId).Writer.WriteAsync(context);
        var res = await syncChannelPool.GetBindChannel(bindId).Reader.ReadAsync();

        // 归还同步channel
        syncChannelPool.ReturnChannel(bindId);

        return res;
    }

    public void SetNextResponse(MessageContext context, Action<string> messageCallback)
    {
        // TODO: Implement this method
        throw new NotImplementedException();
    }

    public bool TryGetNextResponse(Func<MessageContext, string> func)
    {
        MessageContext? context = null;

        foreach (var (key, _) in _bindIds)
        {
            lock (msgChannelPool)
            {
                if (!msgChannelPool.GetBindChannel(key).Reader.TryRead(out context)) continue;

                // 归还消息channel
                msgChannelPool.ReturnChannel(key);
                _bindIds.Remove(key, out _);
            }
        }


        // 如果没有有消息的channel，或者写入失败，返回false
        return context != null &&
               syncChannelPool
                   .GetBindChannel(context.TiedId)
                   .Writer.TryWrite(func(context));
    }
}