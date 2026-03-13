using System.CommandLine;
using Photinizer.Cli.Properties;

const string CliName = "Photinizer.Cli";
string cliAlias = $"{CliName}-{Guid.NewGuid().ToString("N").Substring(0, 6)}";

var sourceOption = new Option<string>("--build-source")
{
    Description = "Path to the project directory (Frontend, wwwroot, configs).",
    Arity = ArgumentArity.ExactlyOne
};

var outOption = new Option<string?>("--out")
{
    Description = "Output directory for bundled artifacts (defaults to <project>/bin/<cfg>/<tfm>/wwwroot).",
    Arity = ArgumentArity.ExactlyOne
};

var rootCommand = new RootCommand("Photinizer bundler CLI")
{
    sourceOption,
    outOption
};

rootCommand.SetAction(parseResult =>
{
    var buildSource = parseResult.GetValue(sourceOption);
    if (string.IsNullOrWhiteSpace(buildSource))
    {
        Console.WriteLine($"[{cliAlias}] --build-source option is required.");
        return 2;
    }
    string sourceDir = Path.GetFullPath(buildSource);
    if (!Directory.Exists(sourceDir))
    {
        Console.WriteLine($"[{cliAlias}] source dir does not exist: {sourceDir}");
        return 3;
    }

    var output = parseResult.GetValue(outOption);
    var outputDir = !string.IsNullOrWhiteSpace(output)
        ? Path.GetFullPath(output)
        : Path.Combine(AppContext.BaseDirectory, "wwwroot");

    Directory.CreateDirectory(outputDir);

    Console.WriteLine($"[{cliAlias}] source = {sourceDir}");
    Console.WriteLine($"[{cliAlias}] out    = {outputDir}");
    Console.Out.Flush();

    return 0;
});

int exitCode = 1;

#if DEBUG
Console.WriteLine($"[{cliAlias}] started.");
#endif

// Run and return exit code.
ParseResult parseResult = rootCommand.Parse(args);
if (parseResult.Errors.Count == 0)
{
    exitCode = parseResult.Invoke();
}
foreach (var parseError in parseResult.Errors)
{
    Console.Error.WriteLine(parseError.Message);
}

if (exitCode == 0 && Resources.ShouldGenerate())
{
    Console.WriteLine($"[{cliAlias}] {Resources.Generate()}");
}

#if DEBUG
Console.WriteLine($"[{cliAlias}] finished. Exit code: {exitCode}");
#endif

return exitCode;