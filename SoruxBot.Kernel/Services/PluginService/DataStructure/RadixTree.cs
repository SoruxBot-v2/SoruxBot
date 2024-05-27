using System.Threading;
namespace SoruxBot.Kernel.Services.PluginService.DataStructure;
public class RadixTree<T>
{
    private readonly RadixTreeNode<T> _root = new(string.Empty);
	private ReaderWriterLockSlim _treeLock = new ReaderWriterLockSlim();

    public RadixTree() { }
	// 前缀匹配，返回所有匹配到的节点值。短路径优先。
	public IEnumerable<T>? PrefixMatch(string path)
	{
		List<T> result = new();
		_treeLock.EnterReadLock();
		try
		{
			PrefixMatchPrivate(_root, path, result);
		}
		finally 
		{
			_treeLock.ExitReadLock(); 
		}
		return result.Count == 0 ? null : result;
	}
    public RadixTree<T> Insert(string path, T value)
    {
		_treeLock.EnterWriteLock();
		try
		{
			if (!TryInsertPrivate(_root, path, value)) throw new ArgumentException($"Argument path ( {path} ) is invalid: Already exists or begin empty", nameof(path));
		}
		finally { _treeLock.ExitWriteLock(); }
        return this;
    }
    public RadixTree<T> Remove(string path)
    {
		_treeLock.EnterWriteLock();
		try
		{
			RemovePrivate(_root, path);
		}
		finally { _treeLock.ExitWriteLock(); }
        return this;
    }
	public RadixTree<T> RemoveByValue(T value)
	{
		// TODO

		return this;
	}
    public T? GetValue(string path)
    {
		_treeLock.EnterReadLock();
		try
		{
			if (TryGetNodePrivate(_root, path, out var node) && node!.IsValueNode)
			{
				return node.Value;
			}
			throw new ArgumentException($"Incorrect path: {path}", "path");
		}
		finally { _treeLock.ExitReadLock(); }
    }
    public bool TryGetValue(string path, out T? value)
    {
		_treeLock.EnterReadLock();
		try
		{
			if (TryGetNodePrivate(_root, path, out var node) && node!.IsValueNode)
			{
				value = node.Value;
				return true;
			}
			value = default;
			return false;
		}
		finally { _treeLock.ExitReadLock(); }
    }
    public bool TryReplace(string path, T value)
    {
		_treeLock.EnterWriteLock();
		try
		{
			if (TryGetNodePrivate(_root, path, out var node) && node!.IsValueNode)
			{
				node.Value = value;
				return true;
			}
			return false;
		}
		finally { _treeLock.ExitWriteLock(); }
    }
    public bool ContainsPath(string path)
    {
		_treeLock.EnterReadLock();
		try
		{
			if (TryGetNodePrivate(_root, path, out var node) && node!.IsValueNode) return true;
			return false;
		}
		finally { _treeLock.ExitReadLock(); }
    }
    public Dictionary<string, T> ToDictionary()
    {
        Dictionary<string, T> dict = new();
		_treeLock.EnterReadLock();
		try
		{
			ToDictionaryPrivate(_root, dict, string.Empty);
		}
		finally { _treeLock.ExitWriteLock(); }
        return dict;
    }

	private static void PrefixMatchPrivate(RadixTreeNode<T> node, string path, List<T> values)
	{
		var keyLength = node.Key.Length;
		// 如果节点与路径前缀匹配，则继续搜索。如果该节点有值，则将该值加入values
		if (path.Length >= keyLength && path.Substring(0, keyLength) == node.Key)
		{
			if (node.IsValueNode) values.Add(node.Value!);
			if (path.Length == keyLength || !node.Children.ContainsKey(path[keyLength])) return;
			PrefixMatchPrivate(node.Children[path[keyLength]], path.Substring(keyLength), values);
		}
		return;
	}
    private static bool TryInsertPrivate(RadixTreeNode<T> node, string path, T value)
    {
        if (path == string.Empty || path == node.Key && node.IsValueNode) return false;
        if (path == node.Key)
        {
            node.SetValue(value);
            return true;
        }
        // 查找新增路径的最长匹配
        int index;
        for (index = 0; index < node.Key.Length && index < path.Length && node.Key[index] == path[index]; index++) ;

        // 新增路径包含了Key，则无需分裂节点
        if (index == node.Key.Length)
        {
            if (node.Children.ContainsKey(path[index]))
            {
                return TryInsertPrivate(node.Children[path[index]], path.Substring(index), value);
            }
            else
            {
                node.Children.Add(path[index], new RadixTreeNode<T>(path.Substring(index), value));
            }
        }
        // 新增路径不包含Key，需要分裂节点
        else
        {
            RadixTreeNode<T> newNode = new(node.Key.Substring(index), node.Children, node.IsValueNode, node.Value);
            node.Key = node.Key.Substring(0, index);
            node.Children = new()
            {
                { newNode.Key[0], newNode }
            };
            if (path.Length > index)
            {
                node.DeleteValue();
                node.Children.Add(path[index], new RadixTreeNode<T>(path.Substring(index), value));
            }
            else
            {
                node.SetValue(value);
            }

        }
        return true;
    }
    private static bool TryGetNodePrivate(RadixTreeNode<T> node, string path, out RadixTreeNode<T>? destNode)
    {
        var keyLength = node.Key.Length;
        if (node.Key == path)
        {
            destNode = node;
            return true;
        }
        if (path.Length <= keyLength || path.Substring(0, keyLength) != node.Key || !node.Children.ContainsKey(path[keyLength]))
        {
            destNode = default;
            return false;
        }
        return TryGetNodePrivate(node.Children[path[keyLength]], path.Substring(keyLength), out destNode);
    }
    private static bool RemovePrivate(RadixTreeNode<T> node, string path)
    {
        bool markAsDeleted = false;
        var keyLength = node.Key.Length;
        if (node.Key == path)
        {
            node.DeleteValue();
        }
        else if (path.Length > keyLength && path.Substring(0, keyLength) == node.Key && node.Children.ContainsKey(path[keyLength]))
        {
            markAsDeleted = RemovePrivate(node.Children[path[keyLength]], path.Substring(keyLength));
        }
        if (markAsDeleted)
        {
            node.Children.Remove(path[keyLength]);
        }
		// 如果只有一个子节点并且该节点不存储值，那么将该节点并入子节点
		if (node.Children.Count == 1 && !node.IsValueNode)
		{
			node.Key += node.Children.Values.First().Key;
			node.Children = node.Children.Values.First().Children;
		}
		// 如果没有子节点，且当前节点也没有值，那么没有必要保留这个节点，将其标记为已删除
		else if (node.Children.Count == 0 && !node.IsValueNode) return true;
        return false;
    }
	private static bool RemoveByValuePrivate(RadixTreeNode<T> node, T value)
	{
		if(node.IsValueNode && value!.Equals(node.Value))
		{
			node.DeleteValue();
		}
		foreach (var child in node.Children)
		{
			if(RemoveByValuePrivate(child.Value, value))
			{
				node.Children.Remove(child.Key);
			}
		}
		// 如果只有一个子节点并且该节点不存储值，那么将该节点并入子节点
		if(node.Children.Count == 1 && !node.IsValueNode)
		{
			node.Key += node.Children.Values.First().Key;
			node.Children = node.Children.Values.First().Children;
		}
		// 如果没有子节点，且当前节点也没有值，那么没有必要保留这个节点，将其标记为已删除
		if (node.Children.Count == 0 && !node.IsValueNode) return true;
		return false;
	}
    private static void ToDictionaryPrivate(RadixTreeNode<T> node, Dictionary<string, T> dict, string pathPrefix)
    {
        if (node.IsValueNode) dict.Add(pathPrefix + node.Key, node.Value!);
        foreach (var child in node.Children)
        {
            ToDictionaryPrivate(child.Value, dict, pathPrefix + node.Key);
        }
    }
}