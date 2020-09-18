 namespace OpenTl.Common.Testing.Logger
 {
     using System;
     using System.Collections.Concurrent;

     using log4net;
     using log4net.Repository;

     using Microsoft.Extensions.Logging;

     public sealed class Log4NetLogger : ILogger
     {
         private readonly ILog _log;

         public Log4NetLogger(ILoggerRepository repository, string category)
         {
             _log = LogManager.GetLogger(repository.Name, category);
         }

         public IDisposable BeginScope<TState>(TState state)
         {
             return null;
         }

         public bool IsEnabled(LogLevel logLevel)
         {
             switch (logLevel)
             {
                 case LogLevel.Critical:
                     return _log.IsFatalEnabled;
                 case LogLevel.Debug:
                 case LogLevel.Trace:
                     return _log.IsDebugEnabled;
                 case LogLevel.Error:
                     return _log.IsErrorEnabled;
                 case LogLevel.Information:
                     return _log.IsInfoEnabled;
                 case LogLevel.Warning:
                     return _log.IsWarnEnabled;
                 default:
                     throw new ArgumentOutOfRangeException(nameof(logLevel));
             }
         }

         public void Log<TState>(LogLevel logLevel,
                                 EventId eventId,
                                 TState state,
                                 Exception exception,
                                 Func<TState, Exception, string> formatter)
         {
             if (!IsEnabled(logLevel))
             {
                 return;
             }

             if (formatter == null)
             {
                 throw new ArgumentNullException(nameof(formatter));
             }

             var message = formatter(state, exception);
             if (!string.IsNullOrEmpty(message) || exception != null)
             {
                 switch (logLevel)
                 {
                     case LogLevel.Critical:
                         _log.Fatal(message);
                         break;
                     case LogLevel.Debug:
                     case LogLevel.Trace:
                         _log.Debug(message);
                         break;
                     case LogLevel.Error:
                         _log.Error(message);
                         break;
                     case LogLevel.Information:
                         _log.Info(message);
                         break;
                     case LogLevel.Warning:
                         _log.Warn(message);
                         break;
                     default:
                         _log.Warn($"Encountered unknown log level {logLevel}, writing out as Info.");
                         _log.Info(message, exception);
                         break;
                 }
             }
         }
     }

     public sealed class Log4NetProvider : ILoggerProvider
     {
         private readonly ILoggerRepository _repository;

         private readonly ConcurrentDictionary<string, Log4NetLogger> _loggers =
             new ConcurrentDictionary<string, Log4NetLogger>();

         public Log4NetProvider(ILoggerRepository repository)
         {
             _repository = repository;
         }

         public ILogger CreateLogger(string categoryName)
         {
             return _loggers.GetOrAdd(_repository.Name, s => new Log4NetLogger(_repository, categoryName));
         }

         public void Dispose()
         {
             _loggers.Clear();
         }
     }
 }