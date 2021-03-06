﻿using System;
using System.IO;

namespace SuperFantasticSteampunk
{
    class Logger
    {
        #region Static Fields
        private static uint messageCounter;
        private static FileStream fileStream;
        private static StreamWriter streamWriter;
        #endregion

        #region Static Methods
        public static void Start()
        {
            messageCounter = 0;
#if WINDOWS
			string logPath = "log.txt";
#else
			string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			string logContainerPath = Path.Combine(documentsPath, "SuperFantasticSteampunk");
			Directory.CreateDirectory(logContainerPath);
			string logPath = Path.Combine(logContainerPath, "log.txt");
#endif
            fileStream = new FileStream(logPath, FileMode.Create, FileAccess.Write);
            streamWriter = new StreamWriter(fileStream);
        }

        public static void Finish()
        {
            streamWriter.Close();
            fileStream.Close();
        }

        public static void Log(string message)
        {
            unchecked { message = messageCounter++.ToString() + ": " + message; }
            streamWriter.WriteLine(message);
#if DEBUG
            Console.WriteLine(message);
#endif
        }

        public static void Log(Exception e)
        {
            Log("Exception: " + e.Message + " (" + e.Source + ") \nTrace:\n" + e.StackTrace);
        }
        #endregion
    }
}
