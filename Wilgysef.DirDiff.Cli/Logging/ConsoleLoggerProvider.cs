using Microsoft.Extensions.Logging;

namespace Wilgysef.DirDiff.Cli.Logging;

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
