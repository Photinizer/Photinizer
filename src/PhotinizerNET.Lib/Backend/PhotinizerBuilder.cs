using PhotinizerNET.Backend.Settings;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PhotinizerNET.Backend;

internal class PhotinizerBuilder(PhotinizerSettings settings, PhotinizerBuildSettings buildSettings)
{
    public void Build()
    {
        Console.WriteLine("Photinizer: Build started...");

        BuildTemplates();
        CreateBundleFile();

        Console.WriteLine("Photinizer: Build done.");
    }

    private void BuildTemplates()
    {
        Console.WriteLine("build templates: started");

        var replacements = new Dictionary<string, string>()
        {
            { "TITLE", settings.Title },
            { "ROOT_COMPONENT", settings.UI.RootComponent },
        };
        BuildTemplate("index.html", replacements);

        Console.WriteLine("build templates: done");
    }

    private void CreateBundleFile()
    {
        Console.WriteLine("build bundle file: started");

        var rootPath = Path.Combine(buildSettings.BuildSource, "Frontend");

        var commonComponentsPath = Path.Combine(rootPath, "components", "common");
        var userComponentsPath = Path.Combine(rootPath, "components", "users");

        var componentFiles = Directory.GetFiles(commonComponentsPath, "*.js", SearchOption.AllDirectories)
                      .Concat(Directory.GetFiles(userComponentsPath, "*.js", SearchOption.AllDirectories)).ToArray();


        var components = new List<Component>();
        foreach (var componentFile in componentFiles)
        {
            var content = File.ReadAllText(componentFile);
            components.Add(new(componentFile, content, GetLinks(componentFile, content)));
        }
        components = SortComponents(components);

        Console.WriteLine("Components found:");
        foreach (var component in components)
            Console.WriteLine($"    {component.FilePath}");


        var componentsBundleContent = string.Join("\n", components.Select(x => x.Content));

        var bundlePath = Path.Combine(AppContext.BaseDirectory, "Frontend", "wwwroot", "photinizer-bundle.js");

        File.WriteAllText(bundlePath, componentsBundleContent);

        Console.WriteLine("build bundle file: done");
    }

    private record Component(string FilePath, string Content, List<string> Dependencies);

    private List<Component> SortComponents(IEnumerable<Component> components)
    {
        var sorted = new List<Component>();
        var visited = new HashSet<string>();
        var visiting = new HashSet<string>(); // Для поиска циклов

        var componentsDict = components.ToDictionary(m => m.FilePath);

        void Visit(Component component)
        {
            if (visited.Contains(component.FilePath)) return;
            if (visiting.Contains(component.FilePath))
                throw new Exception($"Обнаружена циклическая зависимость: {component.FilePath}");

            visiting.Add(component.FilePath);

            foreach (var depName in component.Dependencies)
            {
                if (componentsDict.TryGetValue(depName, out var depComponent))
                    Visit(depComponent);
            }

            visiting.Remove(component.FilePath);
            visited.Add(component.FilePath);
            sorted.Add(component);
        }

        foreach (var component in components)
            Visit(component);

        return sorted;
    }

    private List<string> GetLinks(string filePath, string content)
    {
        var regex = new Regex(@"//\s*using\s+(?<dep>\S+?)(\.js|\s|$)", RegexOptions.Compiled);
        var root = Path.GetDirectoryName(filePath);

        return regex.Matches(content).Select(x => Path.Combine(root, $"{x.Groups["dep"]}.js")).ToList();
    }

    void BuildTemplate(string path, Dictionary<string, string> replacements)
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
