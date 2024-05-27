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
        var messageTask = new Task(() =>
        {
            while (_isRunning)
            {
                var nextMessage = messageQueue.GetNextMessageRequest();
                if (nextMessage == null) continue;
                messageCallback(nextMessage);

                Thread.Sleep(0);
            }
        });
        var responseTask = new Task(() =>
        {
            while (_isRunning)
            {
                responseQueue.TryGetNextResponse(responseCallback);
                Thread.Sleep(0);
            }
        });


        messageTask.Start();
        responseTask.Start();
    }

    public void StopInstance()
    {
        _isRunning = false;
    }
}