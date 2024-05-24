using System;
using System.Threading.Tasks;
using SoruxBot.SDK.Model.Message;

namespace SoruxBot.Kernel.Interface;

public interface IResponseQueue
{
    public Task<string> SetNextResponse(MessageContext context);
    public bool TryGetNextResponse(Func<MessageContext, string> func);
}