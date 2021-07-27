using System;
using System.IO;

namespace ProcessRunner.Diagnostics
{
    public class Logger
    {
        private readonly object m_syncRoot;
        private readonly string m_logFileName;
        private const string timeFormat = "yyyy-MM-dd HH:mm:ss";

        public event Action<string> OnMessage;

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
            WriteFormattedLog(LogLevel.DEBUG, text);
        }

        /// <summary>
        /// Log an ERROR message
        /// </summary>
        /// <param name="text">Message</param>
        public void Error(string text)
        {
            WriteFormattedLog(LogLevel.ERROR, text);
        }

        /// <summary>
        /// Log a FATAL ERROR message
        /// </summary>
        /// <param name="text">Message</param>
        public void Fatal(string text)
        {
            WriteFormattedLog(LogLevel.FATAL, text);
        }

        /// <summary>
        /// Log an INFO message
        /// </summary>
        /// <param name="text">Message</param>
        public void Info(string text)
        {
            WriteFormattedLog(LogLevel.INFO, text);
        }

        /// <summary>
        /// Log a TRACE message
        /// </summary>
        /// <param name="text">Message</param>
        public void Trace(string text)
        {
            WriteFormattedLog(LogLevel.TRACE, text);
        }

        /// <summary>
        /// Log a WARNING message
        /// </summary>
        /// <param name="text">Message</param>
        public void Warning(string text)
        {
            WriteFormattedLog(LogLevel.WARNING, text);
        }

        private void WriteLine(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            WriteConsole(text);
            OnMessage?.Invoke(text);

            lock (m_syncRoot)
            {
                using (var logFileStream = new StreamWriter(File.Open(m_logFileName, FileMode.Append)))
                {
                    logFileStream.WriteLine(text);
                }
            }
        }

        private void WriteConsole(string text)
        {
            Console.WriteLine(text);
        }

        private void WriteFormattedLog(LogLevel level, string text)
        {
            string pretext;
            switch (level)
            {
                case LogLevel.TRACE:
                    pretext = DateTime.Now.ToString(timeFormat) + " [TRACE]   ";
                    break;
                case LogLevel.INFO:
                    pretext = DateTime.Now.ToString(timeFormat) + " [INFO]    ";
                    break;
                case LogLevel.DEBUG:
                    pretext = DateTime.Now.ToString(timeFormat) + " [DEBUG]   ";
                    break;
                case LogLevel.WARNING:
                    pretext = DateTime.Now.ToString(timeFormat) + " [WARNING] ";
                    break;
                case LogLevel.ERROR:
                    pretext = DateTime.Now.ToString(timeFormat) + " [ERROR]   ";
                    break;
                case LogLevel.FATAL:
                    pretext = DateTime.Now.ToString(timeFormat) + " [FATAL]   ";
                    break;
                default:
                    pretext = "";
                    break;
            }

            WriteLine(pretext + text);
        }

        private enum LogLevel
        {
            TRACE,
            INFO,
            DEBUG,
            WARNING,
            ERROR,
            FATAL
        }
    }
}
