using Microsoft.Extensions.Logging;

namespace DirDiff.Cli.Logging;

internal class ConsoleLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new ConsoleLogger();
    }

    public void Dispose()
    {

    }
}
