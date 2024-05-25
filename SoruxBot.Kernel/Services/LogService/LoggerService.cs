using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using SoruxBot.SDK.Plugins.Service;

namespace SoruxBot.Kernel.Services.LogService
{
    public class LoggerService : ILoggerService
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<LoggerService> _logger;
        private readonly ConcurrentDictionary<Type, ILogger> _map = new ();
        
        private static string _loggerPath = Directory.GetCurrentDirectory();

        private string GetCurrentLogFile()
            => Path.Join(_loggerPath,DateTime.Now.ToString("yyyy-MM-dd") + ".log");

        private static void CreateDirIfNotExists(string path)
        {
            path = Path.Join(Directory.GetCurrentDirectory(), path);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        public LoggerService(ILoggerFactory loggerFactory, ILogger<LoggerService> logger)
        {
            this._loggerFactory = loggerFactory;
            this._logger = logger;
            CreateDirIfNotExists("Logs");
            _loggerPath = Path.Join(_loggerPath, "Logs");
        }

        private ILogger GetLoggerObj()
        {
            StackTrace trace = new StackTrace();
            StackFrame? frame = trace.GetFrame(3);
            if (frame == null)
            {
                return _logger;
            }

            MethodBase? logMethod = frame.GetMethod();
            if (logMethod == null)
            {
                return _logger;
            }

            Type? logType = logMethod.ReflectedType;
            if (logType == null)  return _logger;

            return _map.GetOrAdd(logType, sp => _loggerFactory.CreateLogger(sp));
        }

        public ILoggerService Debug(string source, string msg)
        {
            GetLoggerObj().LogDebug($"[{DateTime.Now:HH:mm:ss}][{source}] {msg}");
            File.AppendAllText(GetCurrentLogFile(), $"[{DateTime.Now:HH:mm:ss}][{source}] {msg} \n");
            return this;
        }

        public ILoggerService Debug<T>(string source, string msg, T context)
        {
            GetLoggerObj().LogDebug($"[{DateTime.Now:HH:mm:ss}][{source}] {msg} {context}");
            File.AppendAllText(GetCurrentLogFile(), $"[{DateTime.Now:HH:mm:ss}][{source}] {msg} {context} \n");
            return this;
        }

        public ILoggerService Error(string source, string msg)
        {
            GetLoggerObj().LogError($"[{DateTime.Now:HH:mm:ss}][{source}] {msg}");
            File.AppendAllText(GetCurrentLogFile(), $"[{DateTime.Now:HH:mm:ss}][{source}] {msg} \n");
            return this;
        }

        public ILoggerService Error<T>(string source, string msg, T context)
        {
            GetLoggerObj().LogError($"[{DateTime.Now:HH:mm:ss}][{source}] {msg} {context}");
            File.AppendAllText(GetCurrentLogFile(), $"[{DateTime.Now:HH:mm:ss}][{source}] {msg} {context} \n");
            return this;
        }

        public ILoggerService Error<T>(Exception exception, string source, string msg, T context)
        {
            GetLoggerObj().LogError(exception, $"[{DateTime.Now:HH:mm:ss}][{source}] {msg} {context}");
            File.AppendAllText(GetCurrentLogFile(), $"[{DateTime.Now:HH:mm:ss}][{source}] {msg} {context} \n");
            return this;
        }

        public ILoggerService Fatal(string source, string msg)
        {
            GetLoggerObj().LogCritical($"[{DateTime.Now:HH:mm:ss}][{source}] {msg}");
            File.AppendAllText(GetCurrentLogFile(), $"[{DateTime.Now:HH:mm:ss}][{source}] {msg} \n");
            return this;
        }

        public ILoggerService Fatal<T>(string source, string msg, T context)
        {
            GetLoggerObj().LogCritical($"[{DateTime.Now:HH:mm:ss}][{source}] {msg} {context}");
            File.AppendAllText(GetCurrentLogFile(), $"[{DateTime.Now:HH:mm:ss}][{source}] {msg} {context} \n");
            return this;
        }

        public ILoggerService Fatal<T>(Exception exception, string source, string msg, T context)
        {
            GetLoggerObj().LogCritical(exception, $"[{DateTime.Now:HH:mm:ss}][{source}] {msg} {context}");
            File.AppendAllText(GetCurrentLogFile(), $"[{DateTime.Now:HH:mm:ss}][{source}] {msg} {context}\n");
            return this;
        }

        public ILoggerService Info(string source, string msg)
        {
            GetLoggerObj().LogInformation($"[{DateTime.Now:HH:mm:ss}][{source}] {msg}");
            File.AppendAllText(GetCurrentLogFile(), $"[{DateTime.Now:HH:mm:ss}][{source}] {msg}\n");
            return this;
        }

        public ILoggerService Info<T>(string source, string msg, T context)
        {
            GetLoggerObj().LogInformation($"[{DateTime.Now:HH:mm:ss}][{source}] {msg} {context}");
            File.AppendAllText(GetCurrentLogFile(), $"[{DateTime.Now:HH:mm:ss}][{source}] {msg} {context}\n");
            return this;
        }

        public ILoggerService Warn(string source, string msg)
        {
            GetLoggerObj().LogWarning($"[{DateTime.Now:HH:mm:ss}][{source}] {msg}");
            File.AppendAllText(GetCurrentLogFile(), $"[{DateTime.Now:HH:mm:ss}][{source}] {msg}\n");
            return this;
        }

        public ILoggerService Warn<T>(string source, string msg, T context)
        {
            GetLoggerObj().LogWarning($"[{DateTime.Now:HH:mm:ss}][{source}] {msg} {context}");
            File.AppendAllText(GetCurrentLogFile(), $"[{DateTime.Now:HH:mm:ss}][{source}] {msg} {context}\n");
            return this;
        }
    }
}