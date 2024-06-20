using System.Collections.Concurrent;
using SoruxBot.Kernel.Constant;
using SoruxBot.Kernel.Services.PluginService.DataStructure;
using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Plugins.Service;
using SoruxBot.SDK.Plugins.Model;
using SoruxBot.Kernel.Services.PluginService.Model;
using System.Collections.Generic;

namespace SoruxBot.Kernel.Services.PluginService;

public class PluginsListener(ILoggerService loggerService)
{
	private readonly ConcurrentRadixTree<string, List<PluginsListenerDescriptor>> _matchTree = new();
	
	private readonly ConcurrentDictionary<string, MessageContext?> _contextResult = new ();

	private readonly ReaderWriterLockSlim _listenerLock = new ();
	
    /// <summary>
    /// 进入Filter队列，并且判断是否需要继续执行 Dispatcher
    /// 如果返回 false 那么说明不需要继续进入 Message 路由
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public bool Filter(MessageContext context)
    {
	    using var activity = OpenTelemetryHelper.ActivitySource.StartActivity();
	    
		var path = new List<string>() { context.MessageEventType.ToString() };
		if (context.TargetPlatform != string.Empty)
		{
			path.Add(context.TargetPlatform);
			if (context.TargetPlatformAction != string.Empty)
			{
				path.Add(context.TargetPlatformAction == "GroupMessage" ? "SendGroupMessage" : context.TargetPlatformAction);
			}
		}
		var list = _matchTree.PrefixMatch(path);
		if (list.Length == 0) return true;

		bool isInterceptedToChannel = false;
		foreach (var item in list)
		{
			item.EnterReadLock();
			foreach(var listener in item.Value!)
			{
				if(listener.ConditionCheck(context))
				{
					// 通知给 RegisterListenerAsync 捕获到的 MessageContext
					_contextResult.AddOrUpdate(
						listener.ID, 
						key => context.DeepClone(), 
						(key, existingVal) => existingVal ?? context
					);

					isInterceptedToChannel |= listener.IsInterceptToChannel;
					if (listener.IsInterceptToFilters)
					{
						item.ExitReadLock();
						return !isInterceptedToChannel;
					}
				}
			}
			item.ExitReadLock();
		}
		return !isInterceptedToChannel;
    }
    
    public Task<MessageContext?> RegisterListenerAsync(PluginsListenerDescriptor pluginsListenerDescriptor, CancellationToken cancellationToken = default)
	{
		// 注册到 MessageContext Result
		_contextResult.TryAdd(pluginsListenerDescriptor.ID, null);
		
		// 注册监听树
		var path = new List<string>() { pluginsListenerDescriptor.MessageType.ToString() };
		if (pluginsListenerDescriptor.TargetPlatformType != string.Empty)
		{
			path.Add(pluginsListenerDescriptor.TargetPlatformType);
			if (pluginsListenerDescriptor.TargetAction != string.Empty)
			{
				path.Add(pluginsListenerDescriptor.TargetAction);
			}
		}
		if(_matchTree.TryGetValue(path, out var listenerList))
		{
			listenerList!.EnterWriteLock();
			listenerList.Value!.Add(pluginsListenerDescriptor);
			listenerList.ExitWriteLock();
		}
		else
		{
			var newListenerList = new List<PluginsListenerDescriptor>() { pluginsListenerDescriptor };
			_matchTree.Insert(path, newListenerList);
		}
		
		// 创建一个取消令牌源，带有默认的 60 秒超时
		var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		if (cancellationToken == CancellationToken.None)
		{
			cts.CancelAfter(TimeSpan.FromSeconds(60));
		}
		
		// 注册到树中后开启监听操作，如果监听成功那么进行返回
		return Task.Run(async () =>
		{
			try
			{
				while (!cts.Token.IsCancellationRequested)
				{
					
					// 10 ms 延迟
					await Task.Delay(10, cts.Token);
					
					_contextResult.TryGetValue(pluginsListenerDescriptor.ID, out var ctx);
					
					if (ctx is not null)
					{
						_contextResult.TryRemove(pluginsListenerDescriptor.ID, out var _);
						return ctx;
					}
				}
				
				loggerService.Info(Constant.NameValue.KernelPluginsListenerLogName, "The listener operation timed out.");
			}
			catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
			{
				loggerService.Info(Constant.NameValue.KernelPluginsListenerLogName, "The listener operation timed out.");
			}finally
			{
				cts.Dispose();

				// 移除树结构
				var path = new List<string>() { pluginsListenerDescriptor.MessageType.ToString() };
				if (pluginsListenerDescriptor.TargetPlatformType != string.Empty)
				{
					path.Add(pluginsListenerDescriptor.TargetPlatformType);
					if (pluginsListenerDescriptor.TargetAction != string.Empty)
					{
						path.Add(pluginsListenerDescriptor.TargetAction);
					}
				}
				var listenerList = _matchTree.GetValue(path);
				listenerList!.EnterWriteLock();
				listenerList.Value!.Remove(pluginsListenerDescriptor);
				if (listenerList.Value!.Count == 0) _matchTree.Remove(path);
				listenerList.ExitWriteLock();
			}
			
			return null;
		}, cts.Token);
	}
}