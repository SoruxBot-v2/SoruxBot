using System.Threading.Channels;

namespace SoruxBot.Kernel.Interface;

public interface IChannelPool<T>
{
    public Channel<T> CreateBindChannel(string bindId);
    public bool TryGetBindChannel(string bindId,out Channel<T>? channel);

    public bool ReturnChannel(string bindId);
}