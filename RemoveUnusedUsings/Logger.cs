using System;

namespace RemoveUnusedUsings
{
    public class Logger : ILogger
    {
        private readonly LogLevel logLevel;

        public Logger(LogLevel logLevel)
        {
            this.logLevel = logLevel;
        }

        public void Log(string message)
        {
            if (logLevel == LogLevel.Quiet)
            {
                return;
            }

            Console.WriteLine(message);
        }
    }
}