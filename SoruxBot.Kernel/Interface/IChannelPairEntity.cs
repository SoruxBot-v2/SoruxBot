using System.Threading.Channels;

namespace SoruxBot.Kernel.Interface;

public interface IChannelPairEntity<TI, TO>
{
    public void PushWork(Func<TI, TO> work);
    public Tuple<Channel<TI>, Channel<TO>> GetChannelPair();
}