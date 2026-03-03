using System.Text.RegularExpressions;
using Photinizer.Builder;
using Photinizer.Settings;

namespace Photinizer.UI.Own;

internal partial class PhotinizerOwnUI(string pathToComponents) : IPhotinizerUI
{
    public void Build(PhotinizerSettings settings, PhotinizerBuildOptions buildSettings)
    {
        Console.WriteLine("Photinizer: Build started...");

        BuildTemplates(settings, buildSettings);
        CreateBundleFile(buildSettings);

        Console.WriteLine("Photinizer: Build done.");
    }

    private static void BuildTemplates(PhotinizerSettings settings, PhotinizerBuildOptions buildSettings)
    {
        Console.WriteLine("build templates: started");

        var replacements = new Dictionary<string, string>()
        {
            { "TITLE", settings.Title },
            { "ROOT_COMPONENT", settings.UI.RootComponent },
        };
        foreach ( var file in new[] { "index.html", "photinizer-init.js" })
            BuildTemplate(file, replacements, settings, buildSettings);

        Console.WriteLine("build templates: done");
    }

    private void CreateBundleFile(PhotinizerBuildOptions buildSettings)
    {
        Console.WriteLine($"build bundle file: started. BuildSource: {buildSettings.BuildSource}, pathToComponents: {pathToComponents}");

        var componentsPath = Path.Combine(buildSettings.BuildSource, pathToComponents);

        var commonComponentsPath = Path.Combine(componentsPath, "common");
        var userComponentsPath = Path.Combine(componentsPath, "users");

        var componentFiles = Directory.GetFiles(commonComponentsPath, "*.js", SearchOption.AllDirectories)
                      .Concat(Directory.GetFiles(userComponentsPath, "*.js", SearchOption.AllDirectories)).ToArray();


        var components = new List<Component>();
        foreach (var componentFile in componentFiles)
        {
            var content = File.ReadAllText(componentFile);
            components.Add(new(componentFile, content, GetLinks(componentFile, content)));
        }
        components = ComponentDependencyResolver.OrderComponents(components);

        Console.WriteLine("Components found:");
        foreach (var component in components)
            Console.WriteLine($"    {component.FilePath}");


        var componentsBundleContent = string.Join("\n", components.Select(x => x.Content));

        var bundlePath = Path.Combine(AppContext.BaseDirectory, "Frontend", "wwwroot", "photinizer-bundle.js");

        File.WriteAllText(bundlePath, componentsBundleContent);

        Console.WriteLine("build bundle file: done");
    }

    private static List<string> GetLinks(string filePath, string content)
    {
        var regex = GetLinks();
        var root = Path.GetDirectoryName(filePath);

        return regex.Matches(content).Select(x => Path.Combine(root ?? string.Empty, $"{x.Groups["dep"]}.js")).ToList();
    }

    private static void BuildTemplate(string path, Dictionary<string, string> replacements, PhotinizerSettings settings, PhotinizerBuildOptions buildSettings)
    {
        var subPath = Path.Combine("Frontend", "wwwroot", path);
        var sourcePath = Path.Combine(buildSettings.BuildSource, subPath);
        var targetPath = Path.Combine(AppContext.BaseDirectory, subPath);

        var content = File.ReadAllText(sourcePath);

        foreach (var x in replacements)
            content = content.Replace($"[[{x.Key.ToUpper()}]]", x.Value);
        content = content
            .Replace("[[TITLE]]", settings.Title)
            .Replace("[[ROOT_COMPONENT]]", settings.UI.RootComponent);

        File.WriteAllText(targetPath, content);
    }

    private static void CrashWithError(string file, int line, string message, string code = "PREP001")
    {
        ReportError(file, line, message, code);
        Environment.Exit(1);
    }

    private static void ReportError(string file, int line, string message, string code = "PREP001")
    {
        // Формат: path(line,col): error CODE: message
        // Если путь содержит пробелы, MSBuild все равно его поймет без кавычек здесь
        Console.WriteLine($"{file}({line},1): error {code}: {message}");
    }

    [GeneratedRegex(@"//\s*using\s+(?<dep>\S+?)(\.js|\s|$)", RegexOptions.Compiled)]
    private static partial Regex GetLinks();
}
