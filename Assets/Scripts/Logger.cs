using System;
using System.IO;
using UnityEngine;


namespace Logging
{
    /// <summary>
    /// Handle logging.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// All current log messages.
        /// </summary>
        // private static List<string> logMessages = new List<string>();
        private static bool logging = false;
        /// <summary>
        /// The path to the log file.
        /// </summary>
        private static string path;


        /// <summary>
        /// Start logging messages.
        /// </summary>
        /// <param name="path">The output path.</param>
        public static void StartLogging(string path)
        {
            if (logging)
            {
                return;
            }
            logging = true;
            string directory = Path.GetDirectoryName(path);
            // Create the directory.
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            Logger.path = path;
            Application.logMessageReceived += HandleLog;
            Debug.Log("Started log at: " + DateTime.Now.ToString("MM/dd/yy H:mm:ss"));
        }


        /// <summary>
        /// Stop logging.
        /// </summary>
        public static void StopLogging()
        {
            if (logging)
            {
                logging = false;
                Application.logMessageReceived -= HandleLog;
            }
        }


        /// <summary>
        /// Handle log messages.
        /// </summary>
        /// <param name="logString">The log string.</param>
        /// <param name="stackTrace">The stack trace.</param>
        /// <param name="type">The type of log.</param>
        private static void HandleLog(string logString, string stackTrace, LogType type)
        {
            File.AppendAllLines(path, logString.Split('\n'));
            if (type == LogType.Error || type == LogType.Exception)
            {
                File.AppendAllLines(path, stackTrace.Split('\n'));
            }
        }
    }
}