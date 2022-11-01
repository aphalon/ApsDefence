using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace ApsDefence
{
    public static class Logger
    {
        private static StreamWriter log = (StreamWriter)null;
        private static int m_RowsWritten = 0;
        private static string m_logfile = "";
        private static object locker = new object();
        private static bool m_debugLogging = false;
        private static bool m_logCreationFailed = false;
        private static DateTime m_logDate = DateTime.Now;
        private static bool _cleanupOldLogs = false;

        public static int AutoFlushLoopDelay = 1000;
        private static Timer AutoFlushTimer;

        /// <summary>
        /// 
        /// </summary>
        static Logger()
        {
            AutoFlushTimer = new Timer(AutoFlushTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// 
        /// </summary>
        private static void ResetFlushTimer()
        {
            AutoFlushTimer.Change(AutoFlushLoopDelay, Timeout.Infinite);
        }

        /// <summary>
        /// 
        /// </summary>
        private static void SuspendFlushTimer()
        {
            AutoFlushTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private static void AutoFlushTimerCallback(object state)
        {
            Flush();
            SuspendFlushTimer();

            // Cleanup old log files in our own time
            if (_cleanupOldLogs)
            {
                foreach (string file in Directory.GetFiles(LogDirectory, "*.log"))
                {
                    Match m = Regex.Match(file, @"(\d{4}-\d{2}-\d{2}\.log)$", RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.CreationTime < DateTime.Now.Date.AddDays(-10))
                        {
                            try
                            {
                                fi.Delete();
                            }
                            catch { }
                        }
                    }
                }
                _cleanupOldLogs = false;
            }
        }

        public static string LogDirectory { get; set; } = Helper.ExecutionDirectory;

        public static bool SetDebug
        {
            set
            {
                m_debugLogging = true;
            }
        }

        public static int FlushAfter { get; private set; } = 10;

        private static bool DebugFilePresent
        {
            get
            {
                return File.Exists(Helper.ExecutionDirectory + "Debug.flag");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="obj"></param>
        public static void Log(string message, params object[] obj)
        {
            WriteLog(string.Format(message == null ? "" : message, obj), MessageTypes.Info);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="obj"></param>
        public static void Debug(string message, params object[] obj)
        {
            if (m_debugLogging || DebugFilePresent)
            {
                WriteLog(string.Format(message, obj), MessageTypes.Debug);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="obj"></param>
        public static void Error(string message, params object[] obj)
        {
            WriteLog(string.Format(message, obj), MessageTypes.Error);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="obj"></param>
        public static void Warning(string message, params object[] obj)
        {
            WriteLog(string.Format(message, obj), MessageTypes.Warning);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageType"></param>
        private static void WriteLog(string message, MessageTypes messageType)
        {
            string str = FormatLogMessage(message, messageType);
            lock (locker)
            {
                if (log == null && !m_logCreationFailed || DateTime.Now.Date > m_logDate.Date)
                {
                    CreateLog();
                }
                if (log != null)
                {
                    log.WriteLine("{0} {1}", DateTime.Now.ToString("HH:mm:ss"), str);
                    m_RowsWritten++;
                    ResetFlushTimer();
                    if (m_RowsWritten % FlushAfter == 0)
                    {
                        Flush();
                    }
                }
            }
            Console.WriteLine(str);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageType"></param>
        /// <returns></returns>
        private static string FormatLogMessage(string message, MessageTypes messageType)
        {
            string str = "";
            switch (messageType)
            {
                case MessageTypes.Info:
                    str = "I";
                    break;
                case MessageTypes.Debug:
                    str = "D";
                    break;
                case MessageTypes.Warning:
                    str = "W";
                    break;
                case MessageTypes.Error:
                    str = "E";
                    break;
            }
            return string.Format("[{0:X2}:{1}] {2}", Thread.CurrentThread.ManagedThreadId, str, message);
        }

        /// <summary>
        /// 
        /// </summary>
        private static void CreateLog()
        {
            DateTime now = DateTime.Now;
            string logName = $"{now.ToString("yyyy-MM-dd")}.log";

            try
            {
                if (log != null && now.Date > m_logDate.Date)
                {
                    log.WriteLine("{0} {1}", DateTime.Now.ToString("HH:mm:ss"), FormatLogMessage(string.Format("Spinning log to {0}", logName), MessageTypes.Info));
                    log.Close();
                    log = null;
                }

                if (log != null || m_logCreationFailed)
                {
                    return;
                }

                log = new StreamWriter(Path.Combine(LogDirectory, logName), true);
                m_logfile = logName;
                m_logDate = now;
                _cleanupOldLogs = true;
            }
            catch (Exception ex)
            {
                m_logCreationFailed = true;
                Console.WriteLine("Unable to create logfile: '{0}'", ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Flush()
        {
            if (log != null)
            {
                log.Flush();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Close()
        {
            Log("Logging terminated");
            if (log != null)
            {
                log.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private enum MessageTypes
        {
            Info,
            Debug,
            Warning,
            Error,
        }
    }
}
