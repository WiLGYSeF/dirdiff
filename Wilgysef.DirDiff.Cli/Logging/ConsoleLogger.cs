using Microsoft.Extensions.Logging;

namespace Wilgysef.DirDiff.Cli.Logging;

internal class ConsoleLogger : ILogger
{
    private readonly ConsoleLogFormatter _formatter = new();

    public IDisposable BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Console.Error.WriteLine(_formatter.Format(state));
    }
}
