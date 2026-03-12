using System.CommandLine;

const string CliName = "Photinizer.Cli";


Console.WriteLine($"[{CliName}] enter");
Console.Out.Flush();


var sourceOption = new Option<string>("--build-source")
{
    Description = "Path to the project directory (Frontend, wwwroot, configs).",
    Arity = ArgumentArity.ExactlyOne
};

var outOption = new Option<string?>("--out")
{
    Description = "Output directory for bundled artifacts (defaults to <project>/bin/<cfg>/<tfm>/wwwroot)."
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
        Console.WriteLine($"[{CliName}] --build-source option is required.");
        return 2;
    }
    string sourceDir = Path.GetFullPath(buildSource);
    if (!Directory.Exists(sourceDir))
    {
        Console.WriteLine($"[{CliName}] source dir does not exist: {sourceDir}");
        return 3;
    }

    var output = parseResult.GetValue(outOption);
    var outputDir = !string.IsNullOrWhiteSpace(output)
        ? Path.GetFullPath(output)
        : Path.Combine(AppContext.BaseDirectory, "wwwroot");

    Directory.CreateDirectory(outputDir);

    Console.WriteLine($"[{CliName}] source = {sourceDir}");
    Console.WriteLine($"[{CliName}] out    = {outputDir}");
    Console.Out.Flush();

    return 0;
});

int exitCode = 1;

Console.WriteLine($"[{CliName}] started");

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

Console.WriteLine($"[{CliName}] finished");

return exitCode;