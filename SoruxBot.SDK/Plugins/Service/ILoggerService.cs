namespace SoruxBot.SDK.Plugins.Service
{
    public interface ILoggerService
    {
        void Info(string source, string msg);
        void Warn(string source, string msg);
        void Error(string source, string msg);
        void Fatal(string source, string msg);
        void Debug(string source, string msg);

        void Info<T>(string source, string msg, T context);

        void Warn<T>(string source, string msg, T context);

        void Error<T>(string source, string msg, T context);
        void Fatal<T>(string source, string msg, T context);
        void Debug<T>(string source, string msg, T context);

        void Error<T>(Exception exception, string source, string msg, T context);
        void Fatal<T>(Exception exception, string source, string msg, T context);
    }
}