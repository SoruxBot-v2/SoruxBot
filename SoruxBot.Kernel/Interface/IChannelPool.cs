using System.Threading.Channels;

namespace SoruxBot.Kernel.Interface;

public interface IChannelPool<TI, TO>
{
    public IChannelPairEntity<TI, TO> RentChannelPair(string bindId);
}