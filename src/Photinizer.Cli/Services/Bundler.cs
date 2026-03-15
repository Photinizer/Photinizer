using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Photinizer.Cli.Parsing;
using Photinizer.Settings;

namespace Photinizer.Cli.Services;

public readonly ref partial struct Bundler(PhotinizerConfiguration configuration, string cliAlias, string sourceDir, string outputDir)
{
    public void Build()
    {
        var windows = configuration.Windows;
        if (windows is not { Count: > 0 })
        {
            Console.WriteLine($"[{cliAlias}] nothing to build.");
            return;
        }

        Console.WriteLine($"[{cliAlias}] building configurations ({windows.Count})...");

        StringBuilder? sb = null;
        int i = 0;
        foreach ((string name, var config) in windows)
        {
            Console.WriteLine($"[{cliAlias}] processing configuration #{i++} '{name}'...");
            BuildTemplate(name, config, ref sb);
        }
    }

    private void BuildTemplate(string name, WindowConfiguration config, ref StringBuilder? sb)
    {
        var targetPath = Path.Combine(outputDir, config.Source);
        Debug.Assert(File.Exists(targetPath), $"Target file doesn't exist: {targetPath}");

        if (!File.Exists(targetPath))
        {
            throw new FileNotFoundException($"{config.Source} not found.", targetPath);
        }

        Console.WriteLine($"[{cliAlias}] {name}: build template '{config.Source}'");

        var content = File.ReadAllText(targetPath);

        var parser = new Parser(content);
        var segments = parser.Parse();
        if (segments.Count == 0)
        {
            // nothing to replace; nothing to write
            return;
        }

        var ph = new PlaceholderProvider(
            square: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "TITLE", config.Title } },
            curly:  new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "NAME", name.ToLower() } }
            );

        sb ??= new StringBuilder((int)(content.Length * 1.2));
        sb.Clear();

        bool shouldWriteContent = false;
        foreach (var (kind, placeholder, slice) in segments)
        {
            if (kind == SegmentKind.Text)
            {
                sb.Append(slice.Span);
                continue;
            }

            var replaced = ph.TryGetValue(kind, placeholder.ToString(), out var value);
            sb.Append(replaced ? value : slice.Span); // if no replacement — keep original token
            if (replaced) shouldWriteContent = true;
        }

        var newContent = sb.ToString();

        ReadOnlySpan<char> span = newContent.AsSpan();
        int index;
        if ((index = span.IndexOf("photinizer-init.js".AsSpan(), StringComparison.OrdinalIgnoreCase)) > 0)
        {
            var prefix = GetScriptNamePrefix(span, index);
            var scriptName = $"{prefix}photinizer-init.js";

            var initJs = $"new UI(new {config.UI.RootComponent});{Environment.NewLine}";

            var initJsPath = Path.Combine(outputDir, scriptName);
            Console.WriteLine($"[{cliAlias}] {name}: write file '{initJsPath}'");
            File.WriteAllText(initJsPath, initJs);
        }

        if ((index = span.IndexOf("photinizer-bundle.js".AsSpan(), StringComparison.OrdinalIgnoreCase)) > 0)
        {
            var prefix = GetScriptNamePrefix(span, index);
            var scriptName = $"{prefix}photinizer-bundle.js";

            var bundle = BuildBundle(name, config.UI.RootComponent, sb);
            if (!string.IsNullOrWhiteSpace((bundle)))
            {
                var bundleJsPath = Path.Combine(outputDir, scriptName);
                Console.WriteLine($"[{cliAlias}] {name}: write file '{bundleJsPath}'");
                File.WriteAllText(bundleJsPath, bundle);
            }
            else
            {
                Console.WriteLine($"[{cliAlias}] {name}: empty bundle.");
            }
        }

        if (shouldWriteContent)
        {
            Console.WriteLine($"[{cliAlias}] {name}: write file '{targetPath}'");
            File.WriteAllText(targetPath, newContent);
        }
        else
        {
            Console.WriteLine($"[{cliAlias}] {name}: nothing to write to '{config.Source}'");
        }
    }

    private static ReadOnlySpan<char> GetScriptNamePrefix(ReadOnlySpan<char> html, int index)
    {
        int i = index - 1;
        while (i >= 0 && html[i] != '"' && html[i] != '\'')
            i--;
        return html.Slice(i + 1, index - (i + 1));
    }

    private string? BuildBundle(string name, string rootComponentName, StringBuilder sb)
    {
        var componentsPath = Path.Combine(sourceDir, "components");

        if (!Directory.Exists(componentsPath))
        {
            Console.WriteLine($"[{cliAlias}] components directory does not exist: {componentsPath}");
            return null;
        }

        Console.WriteLine($"[{cliAlias}] {name}: build components...");

        var componentFiles = Directory.GetFiles(componentsPath, "*.js", SearchOption.AllDirectories).ToList();

        var rootComponentFile = FindFilePath(componentFiles, rootComponentName + ".js");

        var rootContent = File.ReadAllText(rootComponentFile);
        var rootComponent = new Component(rootComponentFile, rootContent, GetLinks(rootContent, componentFiles));

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { rootComponent.FilePath };
        var components = new List<Component>() { rootComponent };

        BuildDependencies(rootComponent, components, componentFiles, seen, cliAlias, name);

        var orderedComponents = OrderComponents(components);

        Console.WriteLine($"[{cliAlias}] {name}: Components found ({orderedComponents.Count}):");
        int i = 0;
        sb.Clear();
        foreach (var component in orderedComponents)
        {
            Console.WriteLine($"[{i}]\t{component.FilePath}");
            if (i > 0) sb.AppendLine();
            sb.Append(component.Content);
            i++;
        }

        return sb.ToString();
    }

    private static void BuildDependencies(Component parentComponent, List<Component> components, List<string> componentFiles,
        HashSet<string> seen, string cliAlias, string name)
    {
        if (parentComponent.Dependencies.Count == 0) return;

        foreach (var filePath in parentComponent.Dependencies)
        {
            if (!seen.Add(filePath))
            {
                continue;
            }

            var content = File.ReadAllText(filePath);
            var component = new Component(filePath, content, GetLinks(content, componentFiles));
            components.Add(component);

            BuildDependencies(component, components, componentFiles, seen, cliAlias, name);
        }
    }

    private static string FindFilePath(List<string> componentFiles, string fileName)
    {
        var filePath = componentFiles.FirstOrDefault(x => Path.GetFileName(x).Equals(fileName, StringComparison.OrdinalIgnoreCase));
        return filePath ?? throw new FileNotFoundException($"Component file '{fileName}' not found in components directory.");
    }

    private static List<Component> OrderComponents(List<Component> components)
    {
        var ordered = new List<Component>();
        var visited = new HashSet<string>();
        var visiting = new HashSet<string>(); // For cycle detection

        var componentsDict = components.ToDictionary(m => m.FilePath);

        foreach (var component in components)
        {
            Visit(component);
        }

        return ordered;

        void Visit(Component component)
        {
            if (visited.Contains(component.FilePath)) return;
            if (!visiting.Add(component.FilePath))
            {
                throw new InvalidOperationException($"Circular dependency detected: {component.FilePath}");
            }

            foreach (var depName in component.Dependencies)
            {
                if (componentsDict.TryGetValue(depName, out var depComponent))
                    Visit(depComponent);
            }

            visiting.Remove(component.FilePath);
            visited.Add(component.FilePath);
            ordered.Add(component);
        }
    }

    private static List<string> GetLinks(string content, List<string> componentFiles)
    {
        var regex = GetUsingRegex();

        return [.. regex.Matches(content).Select(x =>
        {
            var fileName = x.Groups["dep"].Value;
            if (!Path.GetExtension(fileName).Equals(".js", StringComparison.CurrentCultureIgnoreCase))
            {
                fileName += ".js";
            }
            return FindFilePath(componentFiles, fileName);
        })];
    }

    [GeneratedRegex(@"//\s*using\s+(?<dep>\S+?)(\.js|\s|$)", RegexOptions.Compiled)]
    private static partial Regex GetUsingRegex();
}

internal readonly record struct Component(string FilePath, string Content, List<string> Dependencies);