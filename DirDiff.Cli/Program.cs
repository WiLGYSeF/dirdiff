using CommandLine;
using DirDiff.Cli.CommandVerbs;
using System.Reflection;

var verbs = LoadVerbs();
await Parser.Default.ParseArguments(args, verbs)
    .WithParsedAsync(Run);

return 0;

static async Task<int> Run(object opts)
{
    return opts switch
    {
        SnapshotOptions snapshotOptions => await SnapshotVerb.Run(snapshotOptions),
        DiffOptions diffOptions => await DiffVerb.Run(diffOptions),
        _ => throw new NotImplementedException(),
    };
}

static Type[] LoadVerbs()
{
    return Assembly.GetExecutingAssembly().GetTypes()
        .Where(t => t.GetCustomAttribute<VerbAttribute>() != null).ToArray();
}