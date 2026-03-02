using Photinizer.Core.Settings;
using System.Text.RegularExpressions;

namespace Photinizer.UI.Own;

internal class PhotinizerOwnUI(string pathToComponents) : IPhotinizerUI
{
    public void Build(PhotinizerSettings settings, PhotinizerBuildSettings buildSettings)
    {
        Console.WriteLine("Photinizer: Build started...");

        BuildTemplates(settings, buildSettings);
        CreateBundleFile(buildSettings);

        Console.WriteLine("Photinizer: Build done.");
    }

    private void BuildTemplates(PhotinizerSettings settings, PhotinizerBuildSettings buildSettings)
    {
        Console.WriteLine("build templates: started");

        var replacements = new Dictionary<string, string>()
        {
            { "TITLE", settings.Title },
            { "ROOT_COMPONENT", settings.UI.RootComponent },
        };
        BuildTemplate("index.html", replacements, settings, buildSettings);

        Console.WriteLine("build templates: done");
    }

    private void CreateBundleFile(PhotinizerBuildSettings buildSettings)
    {
        Console.WriteLine("build bundle file: started");

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

    private List<string> GetLinks(string filePath, string content)
    {
        var regex = new Regex(@"//\s*using\s+(?<dep>\S+?)(\.js|\s|$)", RegexOptions.Compiled);
        var root = Path.GetDirectoryName(filePath);

        return regex.Matches(content).Select(x => Path.Combine(root, $"{x.Groups["dep"]}.js")).ToList();
    }

    void BuildTemplate(string path, Dictionary<string, string> replacements, PhotinizerSettings settings, PhotinizerBuildSettings buildSettings)
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

    static void CrashWithError(string file, int line, string message, string code = "PREP001")
    {
        ReportError(file, line, message, code);
        Environment.Exit(1);
    }

    static void ReportError(string file, int line, string message, string code = "PREP001")
    {
        // Формат: path(line,col): error CODE: message
        // Если путь содержит пробелы, MSBuild все равно его поймет без кавычек здесь
        Console.WriteLine($"{file}({line},1): error {code}: {message}");
    }
}
