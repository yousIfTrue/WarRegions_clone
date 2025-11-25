
namespace Core.Engine
{
    // Simple Debug shim to emulate Unity's Debug class.
    // Provides Log, LogWarning, LogError and an event hook for custom log sinks.
    public static class Debug
    {
        // Event fired for every log entry: (level, message)
        public static event Action<string, string> OnLog;

        // Include timestamps in output
        public static bool IncludeTimestamp { get; set; } = true;

        // Enable/disable console output (useful for tests)
        public static bool ConsoleOutput { get; set; } = true;

        private static string FormatMessage(string level, string message)
        {
            if (IncludeTimestamp)
                return $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
            return $"[{level}] {message}";
        }

        public static void Log(string message)
        {
            var msg = FormatMessage("INFO", message ?? string.Empty);
            try
            {
                OnLog?.Invoke("INFO", message ?? string.Empty);
            }
            catch { /* ignore subscriber exceptions */ }

            if (ConsoleOutput)
            {
                try
                {
                    var prev = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = prev;
                }
                catch
                {
                    // swallow console errors to avoid impacting engine
                }
            }
        }

        public static void LogWarning(string message)
        {
            var msg = FormatMessage("WARN", message ?? string.Empty);
            try
            {
                OnLog?.Invoke("WARN", message ?? string.Empty);
            }
            catch { }

            if (ConsoleOutput)
            {
                try
                {
                    var prev = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = prev;
                }
                catch { }
            }
        }

        public static void LogError(string message)
        {
            var msg = FormatMessage("ERROR", message ?? string.Empty);
            try
            {
                OnLog?.Invoke("ERROR", message ?? string.Empty);
            }
            catch { }

            if (ConsoleOutput)
            {
                try
                {
                    var prev = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine(msg);
                    Console.ForegroundColor = prev;
                }
                catch
                {
                    // swallow
                }
            }
        }

        public static void LogException(Exception ex)
        {
            if (ex == null) return;
            LogError($"{ex.GetType().FullName}: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
