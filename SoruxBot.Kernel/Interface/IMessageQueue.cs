using SoruxBot.SDK.Model.Message;

namespace SoruxBot.Kernel.Interface
{
    public interface IMessageQueue
    {
        /// <summary>
        /// 得到队列中的 Message
        /// </summary>
        /// <returns></returns>
        public MessageContext? GetNextMessageRequest();
        /// <summary>
        /// 向队列中放入Message
        /// </summary>
        /// <param name="value"></param>
        public void SetNextMsg(MessageContext? value);
    }
}