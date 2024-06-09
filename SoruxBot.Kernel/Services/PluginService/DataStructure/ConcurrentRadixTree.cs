
namespace SoruxBot.Kernel.Services.PluginService.DataStructure
{
	/// <summary>
	/// A Radix Tree with Copy-on-Write and Synchronization Mechanism
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ConcurrentRadixTree<TKey, TValue>
		where TKey : notnull
	{
		private static readonly RadixTreeNode<TKey, TValue> _defaultRoot = new RadixTreeNode<TKey, TValue>();
		private RadixTreeNode<TKey, TValue> _root = _defaultRoot;
		private ReaderWriterLockSlim _writeLock = new ReaderWriterLockSlim();
		private ReaderWriterLockSlim _rootLock = new ReaderWriterLockSlim();
		public ConcurrentRadixTree() { }
		public TValue[] PrefixMatch(IEnumerable<TKey> key)
		{
			var result = new List<TValue>();
			_rootLock.EnterReadLock();
			var root = _root;
			_rootLock.ExitReadLock();
			PrefixMatchPrivate(root, key, result);
			return result.ToArray();
		}
		/// <summary>
		/// 添加内容。如果没有路径则创建一个新的路径并添加Value，否则异常。
		/// </summary>
		/// <param name="path"></param>
		/// <param name="value"></param>
		/// <returns>Original object</returns>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public ConcurrentRadixTree<TKey, TValue> Insert(IEnumerable<TKey> key, TValue value)
		{
			ArgumentNullException.ThrowIfNull(key, nameof(key));
			ArgumentNullException.ThrowIfNull(value, nameof(value));
			try
			{
				_writeLock.EnterWriteLock();
				_rootLock.EnterReadLock();
				var root = _root;
				_rootLock.ExitReadLock();
				if (TryInsertPrivate(root, key, value, out var newRoot, false))
				{
					_rootLock.EnterWriteLock();
					_root = newRoot;
					_rootLock.ExitWriteLock();
					return this;
				}
				throw new ArgumentException($"Key {key} already exists.");
			}
			finally { _writeLock.ExitWriteLock(); }
			
		}
		/// <summary>
		/// 添加内容。如果没有路径则创建一个新的路径并添加Value并返回true，否则返回false。
		/// </summary>
		/// <param name="path"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool TryInsert(IEnumerable<TKey> key, TValue value)
		{
			ArgumentNullException.ThrowIfNull(key, nameof(key));
			ArgumentNullException.ThrowIfNull(value, nameof(value));\
			try
			{
				_writeLock.EnterWriteLock();
				_rootLock.EnterReadLock();
				var root = _root;
				_rootLock.ExitReadLock();
				if (TryInsertPrivate(root, key, value, out var newRoot, false))
				{
					_rootLock.EnterWriteLock();
					_root = newRoot;
					_rootLock.ExitWriteLock();
					return true;
				}
				return false;
			}
			finally { _writeLock.ExitWriteLock(); }
		}
		/// <summary>
		/// 添加内容。如果存在原有路径，则覆盖之。
		/// </summary>
		/// <param name="path"></param>
		/// <param name="value"></param>
		/// <returns>Original object</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public ConcurrentRadixTree<TKey, TValue> ForceInsert(IEnumerable<TKey> key, TValue value)
		{
			ArgumentNullException.ThrowIfNull(key, nameof(key));
			ArgumentNullException.ThrowIfNull(value, nameof(value));
			try
			{
				_writeLock.EnterWriteLock();
				_rootLock.EnterReadLock();
				var root = _root;
				_rootLock.ExitReadLock();
				TryInsertPrivate(root, key, value, out var newRoot, true);
				_rootLock.EnterWriteLock();
				_root = newRoot;
				_rootLock.ExitWriteLock();
			}
			finally
			{
				_writeLock.ExitWriteLock();
			}
			return this;
		}
		/// <summary>
		/// 删除路径。合并单分支无值节点，递归删除不存储值的子树。
		/// </summary>
		/// <param name="path"></param>
		/// <returns>Original object</returns>
		/// <exception cref="KeyNotFoundException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public ConcurrentRadixTree<TKey, TValue> Remove(IEnumerable<TKey> key)
		{	
			ArgumentNullException.ThrowIfNull(key, nameof(key));
			try
			{
				_writeLock.EnterWriteLock();
				_rootLock.EnterReadLock();
				var root = _root;
				_rootLock.ExitReadLock();
				if (TryRemovePrivate(root, key, out _, out var newNode))
				{
					_rootLock.EnterWriteLock();
					_root = newNode ?? _defaultRoot;
					_rootLock.ExitWriteLock();
					return this;
				}
				throw new KeyNotFoundException($"Key {key} is not found.");
			}
			finally { _writeLock.ExitWriteLock(); }
		}
		/// <summary>
		/// 删除路径，并通过value返回删除的节点值。合并单分支无值节点，递归删除不存储值的子树。
		/// </summary>
		/// <param name="path"></param>
		/// <param name="value"></param>
		/// <returns>Original object</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="KeyNotFoundException"></exception>
		public ConcurrentRadixTree<TKey, TValue> Remove(IEnumerable<TKey> key, out TValue value)
		{
			ArgumentNullException.ThrowIfNull(key, nameof(key));
			try
			{
				_writeLock.EnterWriteLock();
				_rootLock.EnterReadLock();
				var root = _root;
				_rootLock.ExitReadLock();
				if (TryRemovePrivate(root, key, out value!, out var newNode))
				{
					_rootLock.EnterWriteLock();
					_root = newNode ?? _defaultRoot;
					_rootLock.ExitWriteLock();
					return this;
				}
				throw new KeyNotFoundException($"Key {key} is not found.");
			}
			finally { _writeLock.ExitWriteLock(); }
		}
		/// <summary>
		/// 删除具有对应值的节点。合并单分支无值节点，递归删除不存储值的子树。
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Original object</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public ConcurrentRadixTree<TKey, TValue> Remove(TValue value)
		{
			ArgumentNullException.ThrowIfNull(value, nameof(value));
			try
			{
				_writeLock.EnterWriteLock();
				_rootLock.EnterReadLock();
				var root = _root;
				_rootLock.ExitReadLock();
				if (TryRemoveByValuePrivate(root, value, out var newNode))
				{
					_rootLock.EnterWriteLock();
					_root = newNode ?? _defaultRoot;
					_rootLock.ExitWriteLock();
				}
			}
			finally { _writeLock.ExitWriteLock(); }
			return this;
		}
		/// <summary>
		/// 尝试删除路径。合并单分支无值节点，递归删除不存储值的子树。
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool TryRemove(IEnumerable<TKey> key)
		{
			ArgumentNullException.ThrowIfNull(key, nameof(key));
			try
			{
				_writeLock.EnterWriteLock();
				_rootLock.EnterReadLock();
				var root = _root;
				_rootLock.ExitReadLock();
				if (TryRemovePrivate(root, key, out _, out var newNode))
				{
					_rootLock.EnterWriteLock();
					_root = newNode ?? _defaultRoot;
					_rootLock.ExitWriteLock();
					return true;
				}
			}
			finally { _writeLock.ExitWriteLock(); }
			return false;
		}
		/// <summary>
		/// 更改有值节点的值。
		/// </summary>
		/// <param name="path"></param>
		/// <param name="value"></param>
		/// <returns>Original object</returns>
		/// <exception cref="KeyNotFoundException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public ConcurrentRadixTree<TKey, TValue> SetValue(IEnumerable<TKey> key, TValue value)
		{
			ArgumentNullException.ThrowIfNull(key, nameof(key));
			ArgumentNullException.ThrowIfNull(value, nameof(value));
			try
			{
				_writeLock.EnterWriteLock();
				_rootLock.EnterReadLock();
				var root = _root;
				_rootLock.ExitReadLock();
				if (TrySetPrivate(root, key, value, out var newNode))
				{
					_rootLock.EnterWriteLock();
					_root = newNode;
					_rootLock.ExitWriteLock();
					return this;
				}
				throw new KeyNotFoundException($"Key {key} is not found.");
			}
			finally { _writeLock.ExitWriteLock(); }
		}
		/// <summary>
		/// 更改有值节点的值。
		/// </summary>
		/// <param name="path"></param>
		/// <param name="value"></param>
		/// <returns>Original object</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool TrySetValue(IEnumerable<TKey> key, TValue value)
		{
			ArgumentNullException.ThrowIfNull(key, nameof(key));
			ArgumentNullException.ThrowIfNull(value, nameof(value));
			try
			{
				_writeLock.EnterWriteLock();
				_rootLock.EnterReadLock();
				var root = _root;
				_rootLock.ExitReadLock();
				if (TrySetPrivate(root, key, value, out var newNode))
				{
					_rootLock.EnterWriteLock();
					_root = newNode;
					_rootLock.ExitWriteLock();
					return true;
				}
			}
			finally { _writeLock.ExitWriteLock(); };
			return false;
		}
		/// <summary>
		/// 获取节点值
		/// </summary>
		/// <param name="path"></param>
		/// <returns>Value</returns>
		/// <exception cref="KeyNotFoundException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public TValue GetValue(IEnumerable<TKey> key)
		{
			ArgumentNullException.ThrowIfNull(key, nameof(key));
			_rootLock.EnterReadLock();
			var root = _root;
			_rootLock.ExitReadLock();
			TValue value;
			var node = GetNodePrivate(root, key);
			if(node == null || !node.IsValueNode)
			{
				throw new KeyNotFoundException($"key {key} is not found");
			}
            else
            {
				value = node.Value!;
            }
			return value;
		}
		/// <summary>
		/// 尝试获取节点值，失败则返回false。通过value返回节点值。
		/// </summary>
		/// <param name="path"></param>
		/// <param name="value"></param>
		/// <returns>Success mark</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool TryGetValue(IEnumerable<TKey> key, out TValue? value)
		{
			ArgumentNullException.ThrowIfNull(key, nameof(key));
			_rootLock.EnterReadLock();
			var root = _root;
			_rootLock.ExitReadLock();
			var node = GetNodePrivate(root, key);
			if (node == null || !node.IsValueNode)
			{
				value = default;
				return false;
			}
			value = node.Value;
			return true;
		}
		/// <summary>
		/// 清空内容
		/// </summary>
		public void Clear()
		{
			_writeLock.EnterWriteLock();
			_rootLock.EnterWriteLock();
			_root = _defaultRoot;
			_rootLock.ExitWriteLock();
			_writeLock.ExitWriteLock();
		}
		/// <summary>
		/// 转化为字典
		/// </summary>
		/// <returns>Dictionary of the key-value pairs</returns>
		public Dictionary<IEnumerable<TKey>, TValue> ToDictionary()
		{
			var dict = new Dictionary<IEnumerable<TKey>, TValue>();
			_rootLock.EnterReadLock();
			var root = _root;
			_rootLock.ExitReadLock();
			ToDictionaryPrivate(root, dict, Enumerable.Empty<TKey>());
			return dict;
		}
		private static RadixTreeNode<TKey, TValue>? GetNodePrivate(RadixTreeNode<TKey, TValue> node, IEnumerable<TKey> path)
		{
			var keyLength = node.Key.Count();
			if(path.SequenceEqual(node.Key))
			{
				return node;
			}
			if (path.Count() <= keyLength || !path.Take(keyLength).SequenceEqual(node.Key) || !node.Children.ContainsKey(path.ElementAt(keyLength)))
			{
				return null;
			}
			return GetNodePrivate(node.Children[path.ElementAt(keyLength)], path.Skip(keyLength));
		}
		private static bool TryInsertPrivate(RadixTreeNode<TKey, TValue>? node, IEnumerable<TKey> path, TValue value, out RadixTreeNode<TKey, TValue> newNode, bool replace)
		{
			Dictionary<TKey, RadixTreeNode<TKey, TValue>> newChildren;
			if (node == null)
			{
				newNode = new RadixTreeNode<TKey, TValue>(path, value);
				return true;
			}
			newNode = node;
			var keyLength = node.Key.Count();
			// 如果待插入路径存在节点，并且没有值，或者允许覆盖，则返回一个当前节点有值的拷贝，设置状态为成功。
			if (node.Key.SequenceEqual(path) && !node.IsValueNode || node.Key.SequenceEqual(path) && replace)
			{
				newNode = new RadixTreeNode<TKey, TValue>(path, node.Children, true, value);
				return true;
			}
			if (node.Key.SequenceEqual(path) && node.IsValueNode && !replace) { return false; }
			// 查找新增路径的最长匹配
			int index;
			for (index = 0; index < keyLength && index < path.Count() && node.Key.ElementAt(index).Equals(path.ElementAt(index)); index++) ;

			// 如果最长匹配为Key，则path通向node的子节点
			if(index == keyLength)
			{
				node.Children.TryGetValue(path.ElementAt(index), out var child);
				if (TryInsertPrivate(child, path.Skip(index), value, out var newSubNode, replace))
				{
					newChildren = new Dictionary<TKey, RadixTreeNode<TKey, TValue>>(node.Children);
					newChildren[path.ElementAt(index)] = newSubNode;
					newNode = new RadixTreeNode<TKey, TValue>(
						node.Key,
						new(newChildren),
						node.IsValueNode,
						node.Value
					);
					return true;
				}
				return false;
			}
			// 新增路径不包含Key，则需要分裂节点
			var subNode = new RadixTreeNode<TKey, TValue>(
				node.Key.Skip(index),
				new(node.Children),
				node.IsValueNode,
				node.Value
			);
			newChildren = new Dictionary<TKey, RadixTreeNode<TKey, TValue>>();
			newChildren[node.Key.ElementAt(index)] = subNode;
			newChildren[path.ElementAt(index)] = new RadixTreeNode<TKey, TValue>(path.Skip(index), value);
			newNode = new RadixTreeNode<TKey, TValue>(path.Take(index))
			{
				Children = new(newChildren)
			};
			return true;
		}
		private static bool TrySetPrivate(RadixTreeNode<TKey, TValue> node, IEnumerable<TKey> path, TValue value, out RadixTreeNode<TKey, TValue> newNode)
		{
			Dictionary<TKey, RadixTreeNode<TKey, TValue>> newChildren;
			newNode = node;
			var keyLength = node.Key.Count();
			if (node.Key.SequenceEqual(path) && node.IsValueNode)
			{
				newNode = new RadixTreeNode<TKey, TValue>(path, value)
				{
					Children = new(node.Children)
				};
				return true;
			}
			if (path.Take(keyLength).SequenceEqual(node.Key) && node.Children.TryGetValue(path.ElementAt(keyLength), out var child) && TrySetPrivate(child, path.Skip(keyLength), value, out var newSubNode))
			{
				newChildren = new(node.Children);
				newChildren[path.ElementAt(keyLength)] = newSubNode;
				newNode = new RadixTreeNode<TKey, TValue>(node.Key)
				{
					IsValueNode = node.IsValueNode,
					Value = node.Value,
					Children = new(newChildren)
				};
				return true;
			}
			return false;
		}
		private static bool TryRemovePrivate(RadixTreeNode<TKey, TValue> node, IEnumerable<TKey> path, out TValue? value, out RadixTreeNode<TKey, TValue>? newNode)
		{
			Dictionary<TKey, RadixTreeNode<TKey, TValue>> newChildren;
			value = default;
			newNode = node;
			var keyLength = node.Key.Count();
			if(path.SequenceEqual(node.Key) && node.IsValueNode)
			{
				value = node.Value;
				// 如果node没有子节点，那么没必要保留该节点，直接返回null
				if (node.Children.Count == 0)
				{
					newNode = null;
				}
				// 如果node只有一个子节点，那么将其并入子节点
				else if (node.Children.Count == 1)
				{
					var child = node.Children.Values.First();
					newNode = new RadixTreeNode<TKey, TValue>(Enumerable.Concat(node.Key, child.Key))
					{
						IsValueNode = child.IsValueNode,
						Value = child.Value,
						Children = new(child.Children)
					};
				}
				else newNode = new RadixTreeNode<TKey, TValue>(node.Key)
				{
					IsValueNode = false,
					Value = default,
					Children = new(node.Children)
				};
				return true;
			}
			// 如果path包含Key，则进入子节点递归删除
			if(path.Count() > keyLength && path.Take(keyLength).SequenceEqual(node.Key) && node.Children.ContainsKey(path.ElementAt(keyLength)))
			{
				var success = TryRemovePrivate(node.Children[path.ElementAt(keyLength)], path.Skip(keyLength), out value, out var newSubNode);
				if(success)
				{
					newChildren = new(node.Children);
					if (newSubNode == null)
					{
						newChildren.Remove(path.ElementAt(keyLength));
					}
					else
					{
						newChildren[path.ElementAt(keyLength)] = newSubNode;
					}

					newNode = new RadixTreeNode<TKey, TValue>(node.Key)
					{
						IsValueNode = node.IsValueNode,
						Value = node.Value,
						Children = new(newChildren)
					};
					// 合并节点
					if (!newNode.IsValueNode && newNode.Children.Count == 1)
					{
						var child = newNode.Children.Values.First();
						newNode = new RadixTreeNode<TKey, TValue>(Enumerable.Concat(node.Key, child.Key))
						{
							IsValueNode = child.IsValueNode,
							Value = child.Value,
							Children = new(child.Children)
						};
					}
					return true;
				}
			}
			return false;
		}
		private static bool TryRemoveByValuePrivate(RadixTreeNode<TKey, TValue> node, TValue value, out RadixTreeNode<TKey, TValue>? newNode)
		{
			bool nodeIsValueNode = node.IsValueNode;
			bool success = false;
			TValue? nodeValue = node.Value;
			Dictionary<TKey, RadixTreeNode<TKey, TValue>> newChildren = new(node.Children);
			// 如果还有子节点，那么递归查找符合条件的节点并删除
			if(node.Children.Count > 0)
			{
				foreach (var child in node.Children)
				{
					if (TryRemoveByValuePrivate(child.Value, value, out var subNode))
					{
						success = true;
						if (subNode == null) newChildren.Remove(child.Key);
						else newChildren[child.Key] = subNode;
					}
				}
			}
			// 处理完所有子节点后，检查当前节点
			if(node.IsValueNode && value!.Equals(node.Value))
			{
				success = true;
				nodeIsValueNode = false;
				nodeValue = default;
			}
			if (success)
			{
				// 如果当前节点没有值，且没有子节点，则删除节点；若有一个子节点，则合并节点
				if (!nodeIsValueNode && newChildren.Count == 0) newNode = null;
				else if (!nodeIsValueNode && newChildren.Count == 1)
				{
					var child = newChildren.Values.First();
					newNode = new RadixTreeNode<TKey, TValue>()
					{
						Key = Enumerable.Concat(node.Key, child.Key),
						Value = child.Value,
						IsValueNode = child.IsValueNode,
						Children = new(child.Children)
					};
				}
				else
				{
					newNode = new RadixTreeNode<TKey, TValue>()
					{
						Key = node.Key,
						IsValueNode = nodeIsValueNode,
						Value = nodeValue,
						Children = new(newChildren)
					};
				}
			}
			else newNode = node;
			return success;
		}
		private static void ToDictionaryPrivate(RadixTreeNode<TKey, TValue> node, Dictionary<IEnumerable<TKey>, TValue> dict, IEnumerable<TKey> pathPrefix)
		{
			if (node.IsValueNode) dict.Add(Enumerable.Concat(pathPrefix, node.Key), node.Value!);
			foreach (var child in node.Children)
			{
				ToDictionaryPrivate(child.Value, dict, Enumerable.Concat(pathPrefix, node.Key));
			}
		}
		private static void PrefixMatchPrivate(RadixTreeNode<TKey, TValue> node, IEnumerable<TKey> path, List<TValue> values)
		{
			var keyLength = node.Key.Count();
			// 如果节点与路径前缀匹配，则继续搜索。如果该节点有值，则将该值加入values
			if (path.Count() >= keyLength && path.Take(keyLength).SequenceEqual(node.Key))
			{
				if (node.IsValueNode) values.Add(node.Value!);
				if (path.Count() == keyLength || !node.Children.ContainsKey(path.ElementAt(keyLength))) return;
				PrefixMatchPrivate(node.Children[path.ElementAt(keyLength)], path.Skip(keyLength), values);
			}
			return;
		}
	}
}
