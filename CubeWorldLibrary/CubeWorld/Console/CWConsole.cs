using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeWorld.Console
{
    public class CWConsole : ICWConsoleListener
    {
        private StringBuilder log = new StringBuilder("");
        private string logCopy = null;

        public string TextLog
        {
            get 
            {
                if (logCopy == null)
                    logCopy = log.ToString();

                return logCopy;
            }
        }

        public ICWConsoleListener listener;
        public enum LogLevel
        {
            Info,
            Warning,
            Error
        }

        private CWConsole()
        {
        }

        static private CWConsole singleton;

        static public CWConsole Singleton
        {
            get { if (singleton == null) singleton = new CWConsole(); return singleton; }
        }

        public void Log(CWConsole.LogLevel level, string message)
        {
            log.AppendLine(level.ToString() + " : " + message);
            logCopy = null;

            if (listener != null)
                listener.Log(level, message);
        }

        static public void LogError(string message)
        {
            Singleton.Log(LogLevel.Error, message);
        }

        static public void LogWarning(string message)
        {
            Singleton.Log(LogLevel.Warning, message);
        }

        static public void LogInfo(string message)
        {
            Singleton.Log(LogLevel.Info, message);
        }
    }
}
