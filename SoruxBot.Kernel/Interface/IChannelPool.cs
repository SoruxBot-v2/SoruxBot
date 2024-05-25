using System.Threading.Channels;

namespace SoruxBot.Kernel.Interface;

public interface IChannelPool<T>
{
    public Channel<T> GetBindChannel(string bindId);

    public bool ReturnChannel(string bindId);
}