namespace SoruxBot.Kernel.Services.PluginService.DataStructure;

public class RadixTree<T>
{
    private readonly RadixTreeNode<T> _root = new(string.Empty);

    public RadixTree() { }
	// 前缀匹配，返回所有匹配到的节点值。短路径优先。
	public IEnumerable<T>? PrefixMatch(string path)
	{
		List<T> result = new();
		PrefixMatchPrivate(_root, path, result);
		return result.Count == 0 ? null : result;
	}
    public RadixTree<T> Insert(string path, T value)
    {
        if (!TryInsertPrivate(_root, path, value)) throw new ArgumentException($"Argument path ( {path} ) is invalid: Already exists or begin empty", nameof(path));
        return this;
    }
    public RadixTree<T> Remove(string path)
    {
        RemovePrivate(_root, path);
        return this;
    }
    public T? GetValue(string path)
    {
        if (TryGetNodePrivate(_root, path, out var node) && node!.IsValueNode)
        {
            return node.Value;
        }
        throw new ArgumentException($"Incorrect path: {path}", "path");
    }
    public bool TryGetValue(string path, out T? value)
    {

        if (TryGetNodePrivate(_root, path, out var node) && node!.IsValueNode)
        {
            value = node.Value;
            return true;
        }
        value = default;
        return false;
    }
    public bool TryReplace(string path, T value)
    {
        if (TryGetNodePrivate(_root, path, out var node) && node!.IsValueNode)
        {
            node.Value = value;
            return true;
        }
        return false;
    }
    public bool ContainsPath(string path)
    {
        if (TryGetNodePrivate(_root, path, out var node) && node!.IsValueNode) return true;
        return false;
    }
    public Dictionary<string, T> ToDictionary()
    {
        Dictionary<string, T> dict = new();
        ToDictionaryPrivate(_root, dict, string.Empty);
        return dict;
    }

	private static void PrefixMatchPrivate(RadixTreeNode<T> node, string path, IEnumerable<T> values)
	{
		var keyLength = node.Key.Length;
		// 如果节点与路径前缀匹配，则继续搜索。如果该节点有值，则将该值加入values
		if (path.Length >= keyLength && path.Substring(0, keyLength) == node.Key)
		{
			if (node.IsValueNode) values.Append(node.Value);
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
    private static bool TryGetNodePrivate(RadixTreeNode<T> node, string path, out RadixTreeNode<T> destNode)
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
        if (node.Children.Count == 0) return true;
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