using System.Collections.Concurrent;
using SoruxBot.Kernel.Interface;
using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Plugins.Service;

namespace SoruxBot.Kernel.MessageQueue;

public class ResponseQueueImpl(
    IChannelPool<MessageContext> msgChannelPool,
    IChannelPool<MessageResult> syncChannelPool) : IResponseQueue
{
    private readonly ConcurrentDictionary<string, bool> _bindIds = new();

    // 传递要发送的消息

    // 实现方法同步

    public Task<MessageResult> SetNextResponseAsync(MessageContext context)
    {
        return new Task<Task<MessageResult>>(async () =>
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


    public IResponsePromise SetNextResponse(MessageContext context)
    {

		var promise = new ResponsePromise();

        // 这里是发送给指定用户实体的id
        var bindId = context.TargetPlatform + context.TriggerPlatformId + context.TriggerId;
        _bindIds[bindId] = true;

        new Task<Task>(async () =>
        {
	        Console.WriteLine("要发了~~~");
			if(!msgChannelPool.CreateBindChannel(bindId).Writer.TryWrite(context))
			{
				Console.WriteLine("Error");
			}

			
            var res = await syncChannelPool.CreateBindChannel(bindId).Reader.ReadAsync();

            promise.Callbacks.ForEach(callback => callback(res));
            // 归还同步channel
            syncChannelPool.ReturnChannel(bindId);
        }).Start();

        return promise;
    }


    public bool TryGetNextResponse(Func<MessageContext, MessageResult> func)
    {


	    if (msgChannelPool.TryGetBindChannel("a", out var t))
	    {
		    if (t.Reader.TryRead(out var ctx))
		    {
			    func(ctx);
			    Console.WriteLine(ctx);
		    }

		    return true;
	    }

	    return false;
		    
			MessageContext? context = null;

			foreach (var (key, _) in _bindIds)
			{
				Console.WriteLine("TryGetBindChannel");

				if (!msgChannelPool.TryGetBindChannel(key, out var channel))
				{
					continue;
				}

				if (!channel!.Reader.TryRead(out context))
				{
					Console.WriteLine("aaa");
				}
				else
				{
					Console.WriteLine(context);
				}
				// 归还消息channel
				msgChannelPool.ReturnChannel(key);
				_bindIds.Remove(key, out _);
				break;
			}


			// 如果没有有消息的channel，或者没拿到syncChannel，或者写入失败，返回false
			if (context == null)
			{
				return false;
			}

			Console.WriteLine("进来了");

			if (!syncChannelPool.TryGetBindChannel(
					   context.TargetPlatform + context.TriggerPlatformId + context.TriggerId,
					   out var syncChannel))
			{
				Console.WriteLine("出错了");
			}

			return syncChannel!.Writer.TryWrite(func(context));
		
    }
}