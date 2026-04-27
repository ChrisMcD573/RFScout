using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RFScout.Infrastructure.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _category;
        private readonly StreamWriter _writer;
        private readonly object _lock;
        private readonly FileLoggerOptions _options;

        public FileLogger(string category, StreamWriter writer, object writeLock, FileLoggerOptions options)
        {
            _category = category;
            _writer = writer;
            _lock = writeLock;
            _options = options;
        }
        // public IDisposable BeginScope<TState>(TState state) => NullDisposable.Instance;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return default;
        }
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _options.MinimumLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId,
            TState state, Exception? exception, Func<TState, Exception?, string> formatter )
        {
            if (!IsEnabled(logLevel))
                return;
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var level = logLevel.ToString();
            var message = formatter(state, exception);

            var sb = new StringBuilder();
            sb.Append($"[{timestamp}] ");
            sb.Append($"[{level}] ");

            if (_options.IncludeCategory)
                sb.Append($"[{_category}] ");

            sb.Append(message);

            if (exception != null)
                sb.Append($" Exception: {exception}");

            lock (_lock)
            {
                _writer.WriteLine(sb.ToString());
            }
        }

        private class NullDisposable : IDisposable
        {
            public static NullDisposable Instance { get; } = new NullDisposable();
            private NullDisposable() { }
            public void Dispose() { }
        }

    }
}


/*
 *     Logging: Debugging - (Debug Level) Performance bottlenecks, *important events*, maybe analytics
 *              Analytics/Auditing - (Trace Level) Method entries/exits, detailed state changes, and other fine-grained information that can be useful for troubleshooting and understanding the application's behavior over time.
 *              Basic Monitoring - (Information Level) Startup/Shutdown, general application flow, *significant events*, and other high-level information that can help monitor the application's health and performance in production.
 *              Error and exceptions - (Error Level) Log exceptions and error conditions that occur during the application's execution, including details about the error and potential causes, to aid in troubleshooting and improving the application over time.
 *
 *     What to log:
 *              Startup/shutdown
 *              Background tasks and their results (cache operations, scanning passes)
 *              Error and exceptions
 */

