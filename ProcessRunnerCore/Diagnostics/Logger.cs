using System;
using System.IO;

namespace ProcessRunner.Diagnostics
{
    public class Logger
    {
        private readonly object m_syncRoot;
        private readonly string m_logFileName;
        private const string timeFormat = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// The minimum <see cref="LogLevel"/> required for log messages to be logged.
        /// </summary>
        public LogLevel MinimumLevel
        { get; set; }

        public Logger()
        {
            m_syncRoot = new object();
            m_logFileName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".log.txt";
        }


        /// <summary>
        /// Log a DEBUG message
        /// </summary>
        /// <param name="text">Message</param>
        public void Debug(string text)
        {
            WriteLog(LogLevel.Debug, text);
        }

        /// <summary>
        /// Log an ERROR message
        /// </summary>
        /// <param name="text">Message</param>
        public void Error(string text)
        {
            WriteLog(LogLevel.Error, text);
        }

        /// <summary>
        /// Log a FATAL ERROR message
        /// </summary>
        /// <param name="text">Message</param>
        public void Critical(string text)
        {
            WriteLog(LogLevel.Critical, text);
        }

        /// <summary>
        /// Log an INFO message
        /// </summary>
        /// <param name="text">Message</param>
        public void Info(string text)
        {
            WriteLog(LogLevel.Information, text);
        }

        /// <summary>
        /// Log a TRACE message
        /// </summary>
        /// <param name="text">Message</param>
        public void Trace(string text)
        {
            WriteLog(LogLevel.Trace, text);
        }

        /// <summary>
        /// Log a WARNING message
        /// </summary>
        /// <param name="text">Message</param>
        public void Warning(string text)
        {
            WriteLog(LogLevel.Warning, text);
        }

        private void WriteLog(LogLevel level, string text)
        {
            if (MinimumLevel > level)
                return;

            if (string.IsNullOrEmpty(text))
                return;

            var formattedText = FormatLoggerMessage(level, text);

            Console.WriteLine(formattedText);

            lock (m_syncRoot)
            {
                using (var logFileStream = new StreamWriter(File.Open(m_logFileName, FileMode.Append)))
                {
                    logFileStream.WriteLine(formattedText);
                }
            }
        }

        private string FormatLoggerMessage(LogLevel level, string text)
        {
            var now = DateTime.Now.ToString(timeFormat);
            switch (level)
            {
                case LogLevel.Trace:
                    return $"{now} [TRACE]    {text}";
                case LogLevel.Information:
                    return $"{now} [INFO]     {text}";
                case LogLevel.Debug:
                    return $"{now} [DEBUG]    {text}";
                case LogLevel.Warning:
                    return $"{now} [WARNING]  {text}";
                case LogLevel.Error:
                    return $"{now} [ERROR]    {text}";
                case LogLevel.Critical:
                    return $"{now} [CRITICAL] {text}";
            }

            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Defines logging severity levels.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Logs that contain the most detailed messages. These messages may contain sensitive application data.
        /// These messages are disabled by default and should never be enabled in a production environment.
        /// </summary>
        Trace = 0,

        /// <summary>
        /// Logs that are used for interactive investigation during development.  These logs should primarily contain
        /// information useful for debugging and have no long-term value.
        /// </summary>
        Debug = 1,

        /// <summary>
        /// Logs that track the general flow of the application. These logs should have long-term value.
        /// </summary>
        Information = 2,

        /// <summary>
        /// Logs that highlight an abnormal or unexpected event in the application flow, but do not otherwise cause the
        /// application execution to stop.
        /// </summary>
        Warning = 3,

        /// <summary>
        /// Logs that highlight when the current flow of execution is stopped due to a failure. These should indicate a
        /// failure in the current activity, not an application-wide failure.
        /// </summary>
        Error = 4,

        /// <summary>
        /// Logs that describe an unrecoverable application or system crash, or a catastrophic failure that requires
        /// immediate attention.
        /// </summary>
        Critical = 5,

        /// <summary>
        /// Not used for writing log messages. Specifies that a logging category should not write any messages.
        /// </summary>
        None = 6
    }
}
