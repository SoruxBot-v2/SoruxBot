

namespace SoruxBot.Kernel.Services.PluginService.DataStructure
{
	// 为节点返回值提供一个同步机制。由于返回值类型未知，同步只能由用户完成，因此不能保证一定是线程安全的
	public class RadixTreeNodeValue <TValue>
	{
		private ReaderWriterLockSlim _lock = new();
		public TValue? Value { get; init; }
		public RadixTreeNodeValue(TValue value) => Value = value;
		public void EnterReadLock() => _lock.EnterReadLock();
		public void ExitReadLock() => _lock.ExitReadLock();
		public void EnterWriteLock() => _lock.EnterWriteLock();
		public void ExitWriteLock() => _lock.ExitWriteLock();
		
	}
}
