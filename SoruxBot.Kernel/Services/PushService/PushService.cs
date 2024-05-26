using SoruxBot.Kernel.Interface;
using SoruxBot.SDK.Model.Message;

namespace SoruxBot.Kernel.Services.PushService;

public class PushService(IResponseQueue responseQueue) : IPushService
{
    private bool _isRunning = true;

    public void RunInstance()
    {
        var task = new Task(() =>
        {
            while (_isRunning)
            {
                responseQueue.TryGetNextResponse(context =>
                {
                    // TODO 这里利用MessageContext，从Provider得到MessageId
                    Console.WriteLine(context);
                    const string messageId = "MessageId";
                    Console.WriteLine(messageId);
                    return messageId;
                });
                Thread.Sleep(0);
            }
        });

        task.Start();
    }

    public void StopInstance()
    {
        _isRunning = false;
    }
}