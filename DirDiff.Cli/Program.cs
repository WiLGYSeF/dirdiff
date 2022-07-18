using CommandLine;
using DirDiff.Cli;
using DirDiff.DirMetaSnapshots;
using DirDiff.DirMetaSnapshotWriters;
using DirDiff.Enums;

var result = Parser.Default.ParseArguments<CommandLineOptions>(args);

if (result is Parsed<CommandLineOptions> parsed)
{
    return await Run(result.Value);
}

if (result is NotParsed<CommandLineOptions> notParsed && notParsed.Errors.Any())
{
    HandleParseError(result.Errors);
    return 1;
}

return 0;

async static Task<int> Run(CommandLineOptions opts)
{
    var snapshotBuilder = new DirMetaSnapshotBuilder();
    snapshotBuilder.Configure(options =>
    {
        options.DirectorySeparator = Path.DirectorySeparatorChar;
        options.UseFileSize = true;
        options.UseCreatedTime = true;
        options.UseLastModifiedTime = true;
        options.HashAlgorithm = HashAlgorithm.SHA256;
        options.KeepDirectoryOrder = true;
        options.ThrowIfNotFound = true;
    });

    foreach (var arg in opts.Arguments)
    {
        snapshotBuilder.AddPath(arg);
    }

    try
    {
        var snapshot = snapshotBuilder.CreateSnapshot();

        var snapshotWriter = new DirMetaSnapshotTextWriter();
        snapshotWriter.Configure(options =>
        {
            options.WriteHash = true;
            options.WriteHashAlgorithm = false;
            options.WriteCreatedTime = false;
            options.WriteLastModifiedTime = true;
            options.WriteFileSize = true;

            options.Separator = "  ";
            options.NoneValue = "-";
        });

        await snapshotWriter.WriteAsync(Console.OpenStandardOutput(), snapshot);
    } catch (DirectoryNotFoundException exception)
    {
        WriteError($"could not find path: {exception.Message}");
        return 1;
    }

    return 0;
}

static void HandleParseError(IEnumerable<Error> errors)
{
    foreach (var error in errors)
    {
        Console.WriteLine(error.ToString());
    }
}

static void WriteError(string message)
{
    Console.Error.WriteLine("error: " + message);
}
