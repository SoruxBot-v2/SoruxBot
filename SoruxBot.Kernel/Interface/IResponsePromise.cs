namespace SoruxBot.Kernel.Interface;

public interface IResponsePromise
{
    public void Then(Action<string> messageCallback);
}