namespace ECommerce.Utilities
{
    public static class Logger
    {
        private static readonly string LogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ECommerceApp", "Logs");

        static Logger()
        {
            Directory.CreateDirectory(LogDirectory);
        }

        public static void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        public static void LogWarning(string message)
        {
            WriteLog("WARNING", message);
        }

        public static void LogError(string message, Exception? ex = null)
        {
            var msg = ex != null ? $"{message} | Exception: {ex.Message} | StackTrace: {ex.StackTrace}" : message;
            WriteLog("ERROR", msg);
        }

        private static void WriteLog(string level, string message)
        {
            try
            {
                var logFile = Path.Combine(LogDirectory, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}";
                File.AppendAllText(logFile, logEntry);
            }
            catch { /* Silently fail logging */ }
        }
    }
}
