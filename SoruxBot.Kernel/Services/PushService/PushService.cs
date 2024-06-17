using SoruxBot.Kernel.Interface;
using SoruxBot.SDK.Model.Message;

namespace SoruxBot.Kernel.Services.PushService;

public class PushService(IMessageQueue messageQueue, IResponseQueue responseQueue) : IPushService
{
    private bool _isRunning = true;


    public void RunInstance(
        Action<MessageContext> messageCallback,
        Func<MessageContext, MessageResult> responseCallback)
    {
        responseQueue.SetResponseCallback(responseCallback);

        var messageTask = new Task<Task>(async () =>
        {
            while (_isRunning)
            {
                var nextMessage = await messageQueue.GetNextMessageRequest();
                messageCallback(nextMessage);
                
                //Thread.Sleep(0);
            }
        });

        messageTask.Start();
    }

    public void StopInstance()
    {
        _isRunning = false;
    }
}