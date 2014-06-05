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
            LogEvent(TraceEventType.Critical, null, message, parameters);
        }

        public static void Critical(Exception exception, string message, params object[] parameters)
        {
            LogEvent(TraceEventType.Critical, exception, message, parameters);
        }

        public static void Error(string message, params object[] parameters)
        {
            LogEvent(TraceEventType.Error, null, message, parameters);
        }

        public static void Error(Exception exception, string message, params object[] parameters)
        {
            LogEvent(TraceEventType.Error, exception, message, parameters);
        }

        public static void Warning(string message, params object[] parameters)
        {
            LogEvent(TraceEventType.Warning, null, message, parameters);
        }

        public static void Warning(Exception exception, string message, params object[] parameters)
        {
            LogEvent(TraceEventType.Warning, exception, message, parameters);
        }

        public static void Information(string message, params object[] parameters)
        {
            LogEvent(TraceEventType.Information, null, message, parameters);
        }

        public static void Verbose(string message, params object[] parameters)
        {
            LogEvent(TraceEventType.Verbose, null, message, parameters);
        }

        private static void LogEvent(TraceEventType eventType, Exception exception, string message, params object[] parameters)
        {
            if (parameters.Length > 0)
            {
                message = String.Format(message, parameters);
            }

            var logMessage = String.Format("{0}{1}{2}.", message, (exception == null) ? "" : ": ",
                (exception == null) ? "" : ExtractExceptionMessages(exception));

            var log = GetLogger();
            log._traceSource.TraceEvent(eventType, 0, logMessage);
            log._traceSource.Flush();
            Trace.WriteLine(logMessage);
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
