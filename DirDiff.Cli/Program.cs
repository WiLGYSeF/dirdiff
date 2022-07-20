using CommandLine;
using DirDiff.Cli.CommandLineOptions;
using DirDiff.DirMetaSnapshotDiffWriters;
using DirDiff.DirMetaSnapshotReaders;
using DirDiff.DirMetaSnapshots;
using DirDiff.DirMetaSnapshotWriters;
using DirDiff.Enums;
using System.Reflection;
using System.Text;

var verbs = LoadVerbs();
await Parser.Default.ParseArguments(args, verbs)
    .WithParsedAsync(Run);

return 0;

static async Task<int> Run(object opts)
{
    return opts switch
    {
        SnapshotOptions snapshotOptions => await CreateSnapshot(snapshotOptions),
        DiffOptions diffOptions => await DiffSnapshots(diffOptions),
        _ => throw new NotImplementedException(),
    };
}

static async Task<int> CreateSnapshot(SnapshotOptions opts)
{
    var snapshotBuilder = new DirMetaSnapshotBuilder();
    snapshotBuilder.Configure(options =>
    {
        options.DirectorySeparator = Path.DirectorySeparatorChar;
        options.UseFileSize = opts.UseFileSize;
        options.UseCreatedTime = true;
        options.UseLastModifiedTime = opts.UseLastModifiedTime;
        options.HashAlgorithm = opts.UseHash ? HashAlgorithm.SHA256 : null;
        options.KeepDirectoryOrder = true;
        options.ThrowIfNotFound = true;
    });

    foreach (var arg in opts.Arguments)
    {
        snapshotBuilder.AddPath(arg);
    }

    if (snapshotBuilder.SnapshotPaths.Count == 0)
    {
        foreach (var input in InputFromStream(Console.OpenStandardInput(), opts.NullSeparatedFilenameInput ? 0 : '\n'))
        {
            snapshotBuilder.AddPath(input);
        }
    }

    try
    {
        var snapshot = snapshotBuilder.CreateSnapshot();

        IDirMetaSnapshotWriter? snapshotWriter = opts.SnapshotFormat?.ToLower() switch
        {
            "text" => new DirMetaSnapshotTextWriter().Configure(options =>
            {
                options.Separator = "  ";
                options.NoneValue = "-";
            }),
            "json" => new DirMetaSnapshotJsonWriter().Configure(options =>
            {
                options.UseUnixTimestamp = false;
                options.WriteIndented = true;
            }),
            _ => null,
        };

        if (snapshotWriter == null)
        {
            WriteError("unknown snapshot format");
            return 1;
        }

        snapshotWriter.Configure(options =>
        {
            options.WritePrefix = !opts.RemovePrefix;
            options.WriteHash = opts.UseHash;
            options.WriteHashAlgorithm = false;
            options.WriteCreatedTime = false;
            options.WriteLastModifiedTime = opts.UseLastModifiedTime;
            options.WriteFileSize = opts.UseFileSize;
        });

        await snapshotWriter.WriteAsync(Console.OpenStandardOutput(), snapshot);
    }
    catch (DirectoryNotFoundException exception)
    {
        WriteError($"could not find path: {exception.Message}");
        return 1;
    }

    return 0;
}

static async Task<int> DiffSnapshots(DiffOptions opts)
{
    if (opts.Arguments.Count() != 2)
    {
        WriteError($"diff requires exactly two snapshots to compare.");
        return 1;
    }

    var args = opts.Arguments.ToList();

    var firstPath = args[0];
    var secondPath = args[1];

    DirMetaSnapshot firstSnapshot;
    DirMetaSnapshot secondSnapshot;

    try
    {
        firstSnapshot = await ReadSnapshot(firstPath, opts);
    }
    catch
    {
        WriteError($"could not read snapshot file: {firstPath}");
        return 1;
    }

    try
    {
        secondSnapshot = await ReadSnapshot(secondPath, opts);
    }
    catch
    {
        WriteError($"could not read snapshot file: {secondPath}");
        return 1;
    }

    var diff = secondSnapshot.Compare(
        firstSnapshot,
        !opts.NoSizeAndTimeMatch,
        !opts.UnknownNotModified,
        opts.ModifyWindow.HasValue ? TimeSpan.FromSeconds(opts.ModifyWindow.Value) : null);

    IDirMetaSnapshotDiffWriter? diffWriter = opts.DiffFormat?.ToLower() switch
    {
        "bash" => new DirMetaSnapshotDiffBashWriter(),
        "json" => new DirMetaSnapshotDiffJsonWriter().Configure(options =>
        {
            options.UseUnixTimestamp = false;
            options.WriteIndented = true;
        }),
        _ => null,
    };

    if (diffWriter == null)
    {
        WriteError("unknown diff format");
        return 1;
    }

    diffWriter.Configure(options =>
    {
        options.FirstPrefix = opts.FirstPrefix;
        options.SecondPrefix = opts.SecondPrefix;
    });

    await diffWriter.WriteAsync(Console.OpenStandardOutput(), diff);
    return 0;
}

static async Task<DirMetaSnapshot> ReadSnapshot(string path, DiffOptions opts)
{
    var snapshotJsonReader = new DirMetaSnapshotJsonReader();

    var snapshotTextReader = new DirMetaSnapshotTextReader();
    snapshotTextReader.Configure(options =>
    {
        options.ReadGuess = true;

        options.Separator = "  ";
        options.NoneValue = "-";
    });

    using (var stream = File.OpenRead(path))
    {
        try
        {
            return await snapshotJsonReader.ReadAsync(stream);
        }
        catch
        {
            stream.Position = 0;
            return await snapshotTextReader.ReadAsync(stream);
        }
    }
}

static IEnumerable<string> InputFromStream(Stream stream, int delimiter)
{
    var input = new StringBuilder();
    int @byte;

    while ((@byte = stream.ReadByte()) != -1)
    {
        if (@byte == delimiter)
        {
            if (input.Length > 0)
            {
                yield return input.ToString();
                input.Clear();
            }
        }
        else
        {
            input.Append((char)@byte);
        }
    }

    if (input.Length > 0)
    {
        yield return input.ToString();
    }
}

static void HandleParseError(IEnumerable<Error> errors)
{

}

static void WriteError(string message)
{
    Console.Error.WriteLine("error: " + message);
}

static Type[] LoadVerbs()
{
    return Assembly.GetExecutingAssembly().GetTypes()
        .Where(t => t.GetCustomAttribute<VerbAttribute>() != null).ToArray();
}