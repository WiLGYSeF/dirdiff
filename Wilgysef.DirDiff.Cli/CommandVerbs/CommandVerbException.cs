namespace Wilgysef.DirDiff.Cli.CommandVerbs;

internal class CommandVerbException : Exception
{
    public int ReturnCode { get; }

    public string? Detail { get; }

    public CommandVerbException(int returnCode, string message) : base(message)
    {
        ReturnCode = returnCode;
    }

    public CommandVerbException(int returnCode, string message, string detail) : base(message)
    {
        ReturnCode = returnCode;
        Detail = detail;
    }
}
