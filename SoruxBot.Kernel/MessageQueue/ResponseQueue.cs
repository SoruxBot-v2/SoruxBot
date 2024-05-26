using System.Collections.Concurrent;
using SoruxBot.Kernel.Interface;
using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Model.Utils;

namespace SoruxBot.Kernel.MessageQueue;

public class ResponseQueueImpl(
    IChannelPool<MessageContext> msgChannelPool,
    IChannelPool<string> syncChannelPool) : IResponseQueue
{
    private readonly ConcurrentDictionary<string, bool> _bindIds = new();

    // 传递要发送的消息

    // 实现方法同步

    public Task<string> SetNextResponseAsync(MessageContext context)
    {
        return new Task<Task<string>>(async () =>
        {
            // 这里是发送给指定用户实体的id
            var bindId = context.TargetPlatform + context.TriggerPlatformId + context.TriggerId;
            _bindIds[bindId] = true;


            await msgChannelPool.CreateBindChannel(bindId).Writer.WriteAsync(context);
            var res = await syncChannelPool.CreateBindChannel(bindId).Reader.ReadAsync();

            // 归还同步channel
            syncChannelPool.ReturnChannel(bindId);

            return res;
        }).Unwrap();
    }


    public ResponsePromise SetNextResponse(MessageContext context)
    {
        var promise = new ResponsePromise();

        // 这里是发送给指定用户实体的id
        var bindId = context.TargetPlatform + context.TriggerPlatformId + context.TriggerId;
        _bindIds[bindId] = true;

        new Task<Task>(async () =>
        {
            await msgChannelPool.CreateBindChannel(bindId).Writer.WriteAsync(context);
            var res = await syncChannelPool.CreateBindChannel(bindId).Reader.ReadAsync();

            promise.Callbacks.ForEach(callback => callback(res));
            // 归还同步channel
            syncChannelPool.ReturnChannel(bindId);
        }).Start();

        return promise;
    }


    public bool TryGetNextResponse(Func<MessageContext, string> func)
    {
        MessageContext? context = null;

        foreach (var (key, _) in _bindIds)
        {
            lock (msgChannelPool)
            {
                if (!msgChannelPool.TryGetBindChannel(key, out var channel) ||
                    !channel!.Reader.TryRead(out context)) continue;
                // 归还消息channel
                msgChannelPool.ReturnChannel(key);
                _bindIds.Remove(key, out _);
            }
        }


        // 如果没有有消息的channel，或者没拿到syncChannel，或者写入失败，返回false
        return context != null &&
               syncChannelPool.TryGetBindChannel(context.TiedId, out var syncChannel) &&
               syncChannel!.Writer.TryWrite(func(context));
    }
}