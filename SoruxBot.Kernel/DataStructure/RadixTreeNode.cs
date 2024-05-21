namespace SoruxBot.Kernel.DataStructure;
internal class RadixTreeNode<T>
{
	private bool _isValueNode = false;
	public string Key { get; set; }
	public T? Value { get; set; } = default;
	public Dictionary<char, RadixTreeNode<T>> Children { get; set; } = new();
	public bool IsValueNode { get { return _isValueNode; } }
	public RadixTreeNode(string key)
	{
		this.Key = key;
	}
	public RadixTreeNode(string key, T value)
	{
		this.Key = key;
		this.Value = value;
		_isValueNode = true;
	}
	public RadixTreeNode(string key, Dictionary<char, RadixTreeNode<T>> children, bool isValueNode, T? value)
	{
		this.Key = key;
		this.Children = children;
		this._isValueNode = isValueNode;
		this.Value = value;
	}
	
	public void DeleteValue()
	{
		Value = default;
		_isValueNode = false;
	}
	public void SetValue(T value)
	{
		Value = value;
		_isValueNode = true;
	}
}
