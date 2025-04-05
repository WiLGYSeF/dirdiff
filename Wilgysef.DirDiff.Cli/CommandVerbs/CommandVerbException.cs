namespace Wilgysef.DirDiff.Cli.CommandVerbs;

internal class CommandVerbException : Exception
{
    public int ReturnCode { get; }

    public string? Detail { get; }

    public CommandVerbException(int returnCode, string message, Exception? innerException = null) : base(message, innerException)
    {
        ReturnCode = returnCode;
    }

    public CommandVerbException(int returnCode, string message, string detail, Exception? innerException = null) : base(message, innerException)
    {
        ReturnCode = returnCode;
        Detail = detail;
    }
}
