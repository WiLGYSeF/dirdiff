using CommandLine;
using Wilgysef.DirDiff.Cli.CommandVerbs;
using System.Reflection;

var verbs = LoadVerbs();

try
{
    await Parser.Default.ParseArguments(args, verbs)
        .WithParsedAsync(Run);
}
catch (CommandVerbException exception)
{
    Console.Error.WriteLine("error: " + exception.Message);
    if (exception.Detail != null)
    {
        Console.Error.WriteLine(exception.Detail);
    }
    return exception.ReturnCode;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.ToString());
    return 1;
}

return 0;

static async Task Run(object opts)
{
    switch (opts)
    {
        case SnapshotOptions snapshotOptions:
            await SnapshotVerb.Run(snapshotOptions);
            break;
        case DiffOptions diffOptions:
            await DiffVerb.Run(diffOptions);
            break;
        default:
            throw new NotImplementedException();
    }
}

static Type[] LoadVerbs()
{
    return Assembly.GetExecutingAssembly().GetTypes()
        .Where(t => t.GetCustomAttribute<VerbAttribute>() != null)
        .ToArray();
}