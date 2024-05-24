using SoruxBot.SDK.Model.Message;

namespace SoruxBot.Kernel.Interface.Impl;

public class ResponseQueueImpl:IResponseQueue
{
    public Task<string> SetNextResponse(MessageContext context)
    {
        throw new NotImplementedException();
    }

    public Task<MessageContext?> GetNextResponseAsync(Func<Task<bool>> func)
    {
        throw new NotImplementedException();
    }
}