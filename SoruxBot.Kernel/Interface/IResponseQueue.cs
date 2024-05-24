using SoruxBot.SDK.Model.Message;

namespace SoruxBot.Kernel.Interface;

public interface IResponseQueue
{
    public Task<string> SetNextResponse(MessageContext context);
    public Task<MessageContext?> GetNextResponseAsync(Func<Task<bool>> func);
}