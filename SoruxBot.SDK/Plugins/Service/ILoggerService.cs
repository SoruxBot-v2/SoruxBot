namespace SoruxBot.SDK.Plugins.Service
{
    public interface ILoggerService
    {
        ILoggerService Info(string source, string msg);
        ILoggerService Warn(string source, string msg);
        ILoggerService Error(string source, string msg);
        ILoggerService Fatal(string source, string msg);
        ILoggerService Debug(string source, string msg);

        ILoggerService Info<T>(string source, string msg, T context);

        ILoggerService Warn<T>(string source, string msg, T context);

        ILoggerService Error<T>(string source, string msg, T context);
        ILoggerService Fatal<T>(string source, string msg, T context);
        ILoggerService Debug<T>(string source, string msg, T context);

        ILoggerService Error<T>(Exception exception, string source, string msg, T context);
        ILoggerService Fatal<T>(Exception exception, string source, string msg, T context);
    }
}