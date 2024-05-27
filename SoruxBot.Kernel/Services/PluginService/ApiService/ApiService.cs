using SoruxBot.Kernel.Interface;
using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Plugins.Service;

namespace SoruxBot.Kernel.Services.PluginService.ApiService;

public class ApiService(IResponseQueue queue) : ICommonApi
{
    public Task<MessageResult> SendMessageAsync(MessageContext messageContext)
    {
        return queue.SetNextResponseAsync(messageContext);
    }
    
    public IResponsePromise SendMessage(MessageContext messageContext)
    {
        return queue.SetNextResponse(messageContext);
    }
}