namespace DirDiff.Cli.CommandVerbs;

internal class CommandVerbException : Exception
{
    public int ReturnCode { get; }

    public CommandVerbException(int returnCode, string message) : base(message)
    {
        ReturnCode = returnCode;
    }
}
