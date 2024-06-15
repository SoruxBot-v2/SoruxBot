using System.Collections.Concurrent;
using System.Threading.Channels;
using SoruxBot.Kernel.Interface;
using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Plugins.Service;

namespace SoruxBot.Kernel.MessageQueue;


public class ResponseQueueImpl(
    IChannelPool<MessageContext,MessageResult> msgChannelPool,
    ILoggerService loggerService) : IResponseQueue
{
    private readonly ConcurrentDictionary<string, bool> _bindIds = new();
    
    // 传递要发送的消息

    // 实现方法同步
    // private readonly ManualResetEvent _manualReset = new ManualResetEvent(false);
    
    public Task<MessageResult> SetNextResponseAsync(MessageContext context)
    {
        return new Task<Task<MessageResult>>(async () =>
        {
            // 这里是发送给指定用户实体的id
            var bindId = context.TargetPlatform + context.TriggerPlatformId + context.TriggerId;
            
            var channelPair = msgChannelPool.RentChannelPair(bindId).GetChannelPair();
            await channelPair.Item1.Writer.WriteAsync(context);
            
            msgChannelPool.RentChannelPair(bindId).PushWork(_responseCallback!);

            var res = await channelPair.Item2.Reader.ReadAsync();

            return res;
        }).Unwrap();
    }

    private Func<MessageContext, MessageResult>? _responseCallback;
    
    public IResponsePromise SetNextResponse(MessageContext context)
    {
        
        // 要让插件开发者发送的顺序和到达qq的顺序是一致的，
        // 就是说拿到MessageId，此次消息才算发送完毕
        // 如果此方法被多次调用。需要保证后面的调用在上次调用拿到结果之后才执行
        
        var promise = new ResponsePromise();

        new Task<Task>(async () =>
        {
            try
            {
                // 这里是发送给指定用户实体的id，保证这个顺序一致即可
                // 即能按需发送，又保证一定并发
                var bindId = context.TargetPlatform + context.TriggerPlatformId + context.TriggerId;
            
                var channelPair = msgChannelPool.RentChannelPair(bindId).GetChannelPair();
                await channelPair.Item1.Writer.WriteAsync(context);
                
                msgChannelPool.RentChannelPair(bindId).PushWork(_responseCallback!);

                var res = await channelPair.Item2.Reader.ReadAsync();
                promise.Callbacks.ForEach(callback => callback(res));
            }
            catch (Exception e)
            {
                loggerService.Error("SetNextResponse", "Error when SetNextResponse in promise mode.", e);
            }
           
        }).Start();
        

        return promise;
    }

    public void SetResponseCallback(Func<MessageContext, MessageResult> cb)
    {
        _responseCallback = cb;
    }
}