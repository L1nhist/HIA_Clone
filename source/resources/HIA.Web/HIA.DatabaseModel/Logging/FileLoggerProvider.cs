using Microsoft.Extensions.Logging;
using System;
using System.IO;
namespace HIA.DatabaseModel.Entites
{ 
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _filePath;
        private StreamWriter _writer;

        public FileLoggerProvider(string filePath)
        {
            _filePath = filePath;
            _writer = new StreamWriter(_filePath, append: true) { AutoFlush = true };
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(_writer, categoryName);
        }

        public void Dispose()
        {
            _writer?.Dispose();
        }

        private class FileLogger : ILogger
        {
            private readonly StreamWriter _writer;
            private readonly string _categoryName;

            public FileLogger(StreamWriter writer, string categoryName)
            {
                _writer = writer;
                _categoryName = categoryName;
            }

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {_categoryName}: {formatter(state, exception)}";
                _writer.WriteLine(message);
                if (exception != null)
                    _writer.WriteLine(exception.ToString());
            }
        }
    }
}