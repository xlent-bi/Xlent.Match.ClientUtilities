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
            LogEventCatchAll(TraceEventType.Critical, null, message, parameters);
        }

        public static void Critical(Exception exception, string message, params object[] parameters)
        {
            LogEventCatchAll(TraceEventType.Critical, exception, message, parameters);
        }

        public static void UncaughtException(Exception exception)
        {
            Critical(exception, "Uncaught exception");
        }

        public static void UncaughtException(Exception exception, string format, params object[] parameters)
        {
            Critical(exception, format, parameters);
        }

        public static void Error(string message, params object[] parameters)
        {
            LogEventCatchAll(TraceEventType.Error, null, message, parameters);
        }

        public static void Error(Exception exception, string message, params object[] parameters)
        {
            LogEventCatchAll(TraceEventType.Error, exception, message, parameters);
        }

        public static void Warning(string message, params object[] parameters)
        {
            LogEventCatchAll(TraceEventType.Warning, null, message, parameters);
        }

        public static void Warning(Exception exception, string message, params object[] parameters)
        {
            LogEventCatchAll(TraceEventType.Warning, exception, message, parameters);
        }

        public static void Information(string message, params object[] parameters)
        {
            LogEventCatchAll(TraceEventType.Information, null, message, parameters);
        }

        public static void Information(Exception exception, string message, params object[] parameters)
        {
            LogEventCatchAll(TraceEventType.Information, exception, message, parameters);
        }

        public static void Verbose(string message, params object[] parameters)
        {
            LogEventCatchAll(TraceEventType.Verbose, null, message, parameters);
        }

        private static void LogEventCatchAll(TraceEventType eventType, Exception exception, string format, params object[] parameters)
        {
            try
            {
                var message = parameters.Length > 0 ? String.Format(format, parameters) : format;

                var logMessage = String.Format("{0}{1}{2}.", message, (exception == null) ? "" : ": ",
                    (exception == null) ? "" : ExtractExceptionMessages(exception));
                LogEvent(eventType, logMessage);
            }
            catch (Exception ex)
            {
                try
                {
                    UncaughtException(ex);
                }
                // It is very important that the logging never fails.
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                }
            }
        }

        private static void LogEvent(TraceEventType eventType, string message)
        {
            try
            {
                var log = GetLogger();
                log._traceSource.TraceEvent(eventType, 0, message);
                log._traceSource.Flush();
                WriteForDebug(eventType, message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Uncaught exception: " + ex);
                throw;
            }
        }

        private static void WriteForDebug(TraceEventType eventType, string message)
        {
            var level = "{?}";
            switch (eventType)
            {
                case TraceEventType.Critical:
                    level = "{CRITICAL}";
                    break;
                case TraceEventType.Error:
                    level = "{ERR}";
                    break;
                case TraceEventType.Warning:
                    level = "{WRN}";
                    break;
                case TraceEventType.Information:
                    level = "{I}";
                    break;
                case TraceEventType.Verbose:
                    level = "{V}";
                    break;
             }
            Debug.WriteLine(level + " " + message);
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
