using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Model.Utils;

namespace SoruxBot.Kernel.Interface;

public interface IResponseQueue
{

    public Task<string> SetNextResponseAsync(MessageContext context);

    public ResponsePromise SetNextResponse(MessageContext context);


    public bool TryGetNextResponse(Func<MessageContext, string> func);
}