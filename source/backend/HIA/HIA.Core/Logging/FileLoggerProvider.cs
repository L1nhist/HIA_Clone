using Microsoft.Extensions.Logging;

namespace HIA.Core.Logging;

public class FileLoggerProvider : ILoggerProvider
{
    private readonly StreamWriter _writer;

    public FileLoggerProvider(string filePath)
    {
        _writer = new StreamWriter(filePath, append: true) { AutoFlush = true };
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(_writer, categoryName);
    }

    public void Dispose()
    {
        _writer.Dispose();
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

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {_categoryName}: {formatter(state, exception)}";
            _writer.WriteLine(message);
            if (exception != null)
                _writer.WriteLine(exception.ToString());
        }
    }
}
