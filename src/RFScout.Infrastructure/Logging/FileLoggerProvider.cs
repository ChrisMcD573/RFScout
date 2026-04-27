using System.IO;
using System.Text;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RFScout.Infrastructure.Logging
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly FileLoggerOptions _options;
        private readonly StreamWriter? _writer;
        private readonly object _lock = new object();
        private readonly bool _enabled;

        public FileLoggerProvider(IOptions<FileLoggerOptions> options)
        {
            _options = options.Value;

            try
            {
                // Ensure directory exists
                var directory = Path.GetDirectoryName(_options.FilePath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                // Open file in overwrite mode
                var fileStream = new FileStream(
                    _options.FilePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.Read
                );

                _writer = new StreamWriter(fileStream, Encoding.UTF8)
                {
                    AutoFlush = false // You chose performance over crash‑resilience
                };

                _enabled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[FileLogger] ERROR: Unable to open {_options.FilePath}. " +
                    $"Directory may be missing or permissions denied. " +
                    $"File logging has been disabled. ({ex.Message})"
                );

                _enabled = false;
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            if (!_enabled || _writer == null)
                return new NoOpLogger();

            return new FileLogger(categoryName, _writer, _lock, _options);
        }

        public void Dispose()
        {
            _writer?.Dispose();
        }

        private class NoOpLogger : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) 
    { 
    }
}

        // private class NoOpLogger : ILogger
        // {
        //     public IDisposable BeginScope<TState>(TState state) => default!;
        //     public bool IsEnabled(LogLevel logLevel) => false;
        //     public void Log<TState>(LogLevel logLevel, EventId eventId,
        //         TState state, Exception exception, Func<TState, Exception, string> formatter)
        //     { }
        // }

    }
}