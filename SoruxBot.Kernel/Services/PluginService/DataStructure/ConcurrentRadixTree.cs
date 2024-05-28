using System.Threading;
namespace SoruxBot.Kernel.Services.PluginService.DataStructure
{
	/// <summary>
	/// A Radix Tree with Copy-on-Write and Synchronization Mechanism
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ConcurrentRadixTree<T>
	{
		private RadixTreeNode<T> _root = new(string.Empty);
		private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
		public ConcurrentRadixTree() { }
		/// <summary>
		/// 添加内容。如果没有路径则创建一个新的路径并添加Value，否则异常。
		/// </summary>
		/// <param name="path"></param>
		/// <param name="value"></param>
		/// <returns>Original object</returns>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public ConcurrentRadixTree<T> Insert(string path, T value)
		{
			ArgumentNullException.ThrowIfNull(path, nameof(path));
			ArgumentNullException.ThrowIfNull(value, nameof(value));
			_lock.EnterWriteLock();
			try
			{
				var success = TryInsertPrivate(_root, path, value, out var newRoot);
				if (success)
				{
					_root = newRoot;
					return this;
				}
				throw new ArgumentException($"Key {path} already exists.");
			}
			finally { _lock.ExitWriteLock(); }
		}
		/// <summary>
		/// 添加内容。如果没有路径则创建一个新的路径并添加Value并返回true，否则返回false。
		/// </summary>
		/// <param name="path"></param>
		/// <param name="value"></param>
		/// <returns>Original object</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool TryInsert(string path, T value)
		{
			ArgumentNullException.ThrowIfNull(path, nameof(path));
			ArgumentNullException.ThrowIfNull(value, nameof(value));
			_lock.EnterWriteLock();
			try
			{
				var success = TryInsertPrivate(_root, path, value, out var newRoot);
				if (success)
				{
					_root = newRoot;
					return true;
				}
				return false;
			}
			finally { _lock.ExitWriteLock(); }
		}
		/// <summary>
		/// 删除路径。合并单分支无值节点，递归删除不存储值的子树。
		/// </summary>
		/// <param name="path"></param>
		/// <returns>Original object</returns>
		/// <exception cref="KeyNotFoundException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public ConcurrentRadixTree<T> Remove(string path)
		{	
			ArgumentNullException.ThrowIfNull(path, nameof(path));
			return this;
		}
		/// <summary>
		/// 删除路径，并通过value返回删除的节点值。合并单分支无值节点，递归删除不存储值的子树。
		/// </summary>
		/// <param name="path"></param>
		/// <param name="value"></param>
		/// <returns>Original object</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="KeyNotFoundException"></exception>
		public ConcurrentRadixTree<T> Remove(string path, out T value)
		{
			ArgumentNullException.ThrowIfNull(path, nameof(path));
			value = default;
			return this;
		}
		/// <summary>
		/// 删除具有对应值的节点。合并单分支无值节点，递归删除不存储值的子树。
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Original object</returns>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public ConcurrentRadixTree<T> Remove(T value)
		{
			ArgumentNullException.ThrowIfNull(value, nameof(value));
			return this;
		}
		/// <summary>
		/// 更改有值节点的值。
		/// </summary>
		/// <param name="path"></param>
		/// <param name="value"></param>
		/// <returns>Original object</returns>
		/// <exception cref="KeyNotFoundException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public ConcurrentRadixTree<T> SetValue(string path, T value)
		{
			ArgumentNullException.ThrowIfNull(path, nameof(path));
			ArgumentNullException.ThrowIfNull(value, nameof(value));
			return this;
		}
		/// <summary>
		/// 获取节点值
		/// </summary>
		/// <param name="path"></param>
		/// <returns>Value</returns>
		/// <exception cref="KeyNotFoundException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public T GetValue(string path)
		{
			ArgumentNullException.ThrowIfNull(path, nameof(path));
			_lock.EnterReadLock();
			T value;
			try
			{
				var node = GetNodePrivate(_root, path);
				if(node == null || !node.IsValueNode)
				{
					throw new KeyNotFoundException($"key {path} is not found");
				}
                else
                {
					value = node.Value!;
                }
            }
			finally { _lock.ExitReadLock(); }
			return value;
		}
		/// <summary>
		/// 尝试获取节点值，失败则返回false。通过value返回节点值。
		/// </summary>
		/// <param name="path"></param>
		/// <param name="value"></param>
		/// <returns>Success mark</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool TryGetValue(string path, out T? value)
		{
			ArgumentNullException.ThrowIfNull(path, nameof(path));
			_lock.EnterReadLock();
			try
			{
				var node = GetNodePrivate(_root, path);
				if (node == null)
				{
					value = default;
					return false;
				}
				value = node.Value;
				return true;
			}
			finally { _lock.ExitReadLock(); }
		}
		/// <summary>
		/// 清空内容
		/// </summary>
		public void Clear()
		{
			_root = new RadixTreeNode<T>(string.Empty);
		}
		/// <summary>
		/// 转化为字典
		/// </summary>
		/// <returns>Dictionary of the key-value pairs</returns>
		public Dictionary<string, T> ToDictionary()
		{
			return new Dictionary<string, T>();
		}
		private static RadixTreeNode<T>? GetNodePrivate(RadixTreeNode<T> node, string path)
		{
			var keyLength = node.Key.Length;
			if(node.Key == path)
			{
				return node;
			}
			if (path.Length <= keyLength || path.Substring(0, keyLength) != node.Key || !node.Children.ContainsKey(path[keyLength]))
			{
				return null;
			}
			return GetNodePrivate(node.Children[path[keyLength]], path.Substring(keyLength));
		}
		private static bool TryInsertPrivate(RadixTreeNode<T>? node, string path, T value, out RadixTreeNode<T> newNode)
		{
			if (node == null)
			{
				newNode = new RadixTreeNode<T>(path, value);
				return true;
			}
			newNode = node;
			var keyLength = node.Key.Length;
			// 如果待插入路径存在节点，并且没有值，则返回一个当前节点有值的拷贝，设置状态为成功。
			if (node.Key == path && !node.IsValueNode)
			{
				newNode = new RadixTreeNode<T>(path, value);
				newNode.Children = new(node.Children);
				return true;
			}
			if (node.Key == path && node.IsValueNode) { return false; }
			// 查找新增路径的最长匹配
			int index;
			for (index = 0; index < keyLength && index < path.Length && node.Key[index] == path[index]; index++) ;

			// 如果最长匹配为Key，则path通向node的子节点
			if(index == keyLength)
			{
				node.Children.TryGetValue(path[index], out var child);
				if (TryInsertPrivate(child, path.Substring(index), value, out var newSubNode))
				{
					newNode = new RadixTreeNode<T>(node.Key) { 
						IsValueNode = node.IsValueNode, 
						Value = node.Value,
						Children = new(node.Children)
					};
					newNode.Children[path[index]] = newSubNode;
					return true;
				}
				return false;
			}
			// 新增路径不包含Key，则需要分裂节点
			var subNode = new RadixTreeNode<T>(node.Key.Substring(index))
			{
				IsValueNode = node.IsValueNode,
				Value = node.Value,
				Children = new(node.Children)
			};
			newNode = new RadixTreeNode<T>(path.Substring(0, index));
			newNode.Children[node.Key[index]] = subNode;
			newNode.Children[path[index]] = new RadixTreeNode<T>(path.Substring(index), value);
			return true;
		}
		private static bool TrySetPrivate(RadixTreeNode<T> node, string path, T value, out RadixTreeNode<T> newNode)
		{

			newNode = node;
			var keyLength = node.Key.Length;
			if (node.Key == path && node.IsValueNode)
			{
				newNode = new RadixTreeNode<T>(path, value)
				{
					Children = new(node.Children)
				};
				return true;
			}
			if (path.Substring(0, keyLength) == node.Key && node.Children.TryGetValue(path[keyLength], out var child) && TrySetPrivate(child, path.Substring(keyLength), value, out var newSubNode))
			{
				newNode = new RadixTreeNode<T>(node.Key)
				{
					IsValueNode = node.IsValueNode,
					Value = node.Value,
					Children = new(node.Children)
				};
				newNode.Children[path[keyLength]] = newSubNode;
				return true;
			}
			return false;
		}
	}
}
