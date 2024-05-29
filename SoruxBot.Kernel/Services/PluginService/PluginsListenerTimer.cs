using System;
using System.Threading;
using SoruxBot.Kernel.Bot;
using SoruxBot.Kernel.Services.PluginService.DataStructure;
using SoruxBot.Kernel.Services.PluginService.Model;
using SoruxBot.SDK.Plugins.Model;
using SoruxBot.SDK.Plugins.Service;

namespace SoruxBot.Kernel.Services.PluginService
{
	public class PluginsListenerTimer
	{
		private readonly BotContext _botContext;
		private readonly ILoggerService _loggerService;
		private RadixTree<PluginsListenerDescriptor> _matchTree;

		private PriorityQueue<PluginsListenerDescriptor, DateTime> _listenerQueue = new();
		private Timer _timer;

		public PluginsListenerTimer(BotContext botContext, ILoggerService loggerService, RadixTree<PluginsListenerDescriptor> matchTree)
		{
			_botContext = botContext;
			_loggerService = loggerService;
			_matchTree = matchTree;
			_timer = new Timer(CheckIfTimeout!, new List<object>(){ _listenerQueue, _matchTree }, Timeout.Infinite, Timeout.Infinite);
		}
		public void StartTimer()
		{
			_timer.Change(Timeout.Infinite, 1);
		}
		public void StopTimer()
		{
			_timer.Change(Timeout.Infinite, Timeout.Infinite);
		}
		private static void CheckIfTimeout(object state)
		{
			var pqueue = (PriorityQueue<PluginsListenerDescriptor, DateTime>)((List<object>)state)[0];
			var tree = (RadixTree<PluginsListenerDescriptor>)((List<object>)state)[1];
			while (pqueue.Count > 0)
			{
				var listener = pqueue.Peek();
				if (DateTime.Now > listener.Timeout)
				{
					pqueue.Dequeue();
					// TODO 并发处理
					tree.RemoveByValue(listener);
				}
				else break;
			}
		}
	}
}
