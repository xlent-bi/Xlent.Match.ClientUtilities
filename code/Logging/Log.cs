using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Xlent.Match.ClientUtilities.Logging
{
    public class Log
    {
        private readonly TraceSource _traceSource;
        private readonly static Dictionary<string, Log> Loggers = new Dictionary<string, Log>();
        private readonly static object LoggerLock = new object();

        private Log(string logName)
        {
            _traceSource = new TraceSource(logName);
        }

        private static Log GetLogger(string name = null)
        {
            var logName = name ?? Assembly.GetCallingAssembly().GetName().Name;
            Log log;
            lock (LoggerLock)
            {
                if (Loggers.TryGetValue(logName, out log)) return log;

                log = new Log(logName);
                Loggers.Add(logName, log);
            }
            return log;
        }

        public static void Critical(string message, params object[] parameters)
        {
            LogEvent(TraceEventType.Critical, message, null);
        }

        public static void Critical(Exception exception, string message, params object[] parameters)
        {
            LogEvent(TraceEventType.Critical, message, exception);
        }

        public static void Error(string message, params object[] parameters)
        {
            LogEvent(TraceEventType.Error, message, null);
        }

        public static void Error(Exception exception, string message, params object[] parameters)
        {
            LogEvent(TraceEventType.Error, message, exception);
        }

        public static void Warning(string message, params object[] parameters)
        {
            LogEvent(TraceEventType.Warning, message, null);
        }

        public static void Warning(Exception exception, string message, params object[] parameters)
        {
            LogEvent(TraceEventType.Warning, message, exception);
        }

        public static void Information(string message, params object[] parameters)
        {
            LogEvent(TraceEventType.Information, String.Format(message, parameters));
        }

        public static void Verbose(string message, params object[] parameters)
        {
            LogEvent(TraceEventType.Verbose, message);
        }

        private static void LogEvent(TraceEventType eventType, string message, Exception exception = null)
        {
            var logMessage = String.Format("{0}{1}{2}.", message, (exception == null) ? "" : ": ",
                (exception == null) ? "" : ExtractExceptionMessages(exception));

            var log = GetLogger();
            log._traceSource.TraceEvent(eventType, 0, logMessage);
            log._traceSource.Flush();
        }

        private static string ExtractExceptionMessages(Exception ex)
        {
            var messages = ex.ToString();
            for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
            {
                messages = string.Format("{0};{1}", messages, inner);
            }
            return messages;
        }
    }
}
