using System;
using System.Threading.Tasks;
using SoruxBot.SDK.Model.Message;

namespace SoruxBot.Kernel.Interface;

public interface IResponseQueue
{
    
    public Task<string> SetNextResponse(MessageContext context);
    // <summary>
    // 设置下一个响应
    // </summary>
    // <param name="context"></param>
    // <param name="messageCallback">
    // 得到MessageId后的处理逻辑
    // </param>
    public void SetNextResponse(MessageContext context, Action<string> messageCallback);
    public bool TryGetNextResponse(Func<MessageContext, string> func);
}