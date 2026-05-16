namespace WorkerServiceDemo.Services
{
    public static class LoggerExtension
    {
        public static void Log(this string message, string backupDirectory)
        {
            if (string.IsNullOrEmpty(backupDirectory)) return;

            try
            {
                if (!Directory.Exists(backupDirectory))
                {
                    Directory.CreateDirectory(backupDirectory);
                }
                File.AppendAllText(Path.Combine(backupDirectory, "logfile.txt"), $"{DateTime.Now} : {message}{Environment.NewLine}");
            }
            catch
            {
                // Ignore logging errors to prevent service failure
            }
        }
    }
}