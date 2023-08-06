﻿namespace WorkerServiceDemo.Services
{
    public static class LoggerExtension
    {
        public static void Log(this string message)
        {
            File.AppendAllText($"{Constants.BackUpDirectory}{Path.DirectorySeparatorChar}logfile.txt", $"{DateTime.Now} : {message}{Environment.NewLine}");
        }
    }
}