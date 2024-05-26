using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using SoruxBot.SDK.Model.Message;

namespace SoruxBot.Kernel.Interface;

public interface IResponseQueue
{

    public Task<string> SetNextResponseAsync(MessageContext context);

    public IResponsePromise SetNextResponse(MessageContext context);


    public bool TryGetNextResponse(Func<MessageContext, string> func);
}