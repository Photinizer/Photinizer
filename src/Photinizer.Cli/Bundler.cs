using System.Diagnostics;
using System.Text.RegularExpressions;
using Photinizer.Settings;

namespace Photinizer.Cli
{
    public ref partial struct Bundler(PhotinizerConfiguration configuration, string cliAlias, string sourceDir, string outputDir)
    {
        public void BuildTemplates()
        {
            Console.WriteLine($"[{cliAlias}] build templates...");

            Debug.Assert(configuration.Windows.ContainsKey("MainWindow"));
            var settings = configuration.Windows["MainWindow"];

            var replacements = new Dictionary<string, string>()
            {
                { "TITLE", settings.Title },
                { "ROOT_COMPONENT", settings.UI.RootComponent },
            };
            foreach (var fileName in new[] { settings.Source, "photinizer-init.js" })
                BuildTemplate(fileName, replacements, settings);
        }

        private void BuildTemplate(string fileName, Dictionary<string, string> replacements, WindowConfiguration settings)
        {
            var targetPath = Path.Combine(outputDir, fileName);
            Debug.Assert(File.Exists(targetPath), $"Target file doesn't exist: {targetPath}");

            Console.WriteLine($"[{cliAlias}] build '{targetPath}'");

            var content = File.ReadAllText(targetPath);

            foreach (var x in replacements)
                content = content.Replace($"[[{x.Key.ToUpper()}]]", x.Value);
            content = content
                .Replace("[[TITLE]]", settings.Title)
                .Replace("[[ROOT_COMPONENT]]", settings.UI.RootComponent);

            File.WriteAllText(targetPath, content);
        }

        public void CreateBundleFile()
        {
            var componentsPath = Path.Combine(sourceDir, "components");

            if (!Directory.Exists(componentsPath))
            {
                Console.WriteLine($"[{cliAlias}] components directory does not exist: {componentsPath}");
                return;
            }

            Console.WriteLine($"[{cliAlias}] build components...");

            var componentFiles = Directory.GetFiles(componentsPath, "*.js", SearchOption.AllDirectories);

            var components = new List<Component>();
            foreach (var componentFile in componentFiles)
            {
                var content = File.ReadAllText(componentFile);
                components.Add(new(componentFile, content, GetLinks(componentFile, content)));
            }
            components = OrderComponents(components);

            Console.WriteLine("Components found:");
            foreach (var component in components)
                Console.WriteLine($"    {component.FilePath}");

            var componentsBundleContent = string.Join("\n", components.Select(x => x.Content));

            var bundlePath = Path.Combine(outputDir, "photinizer-bundle.js");
            File.WriteAllText(bundlePath, componentsBundleContent);
        }

        private static List<Component> OrderComponents(IEnumerable<Component> components)
        {
            var sorted = new List<Component>();
            var visited = new HashSet<string>();
            var visiting = new HashSet<string>(); // For cycle detection

            var componentsDict = components.ToDictionary(m => m.FilePath);

            void Visit(Component component)
            {
                if (visited.Contains(component.FilePath)) return;
                if (visiting.Contains(component.FilePath))
                    throw new Exception($"Circular dependency detected: {component.FilePath}");

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
            var regex = GetLinks();
            var root = Path.GetDirectoryName(filePath);

            return regex.Matches(content).Select(x => Path.Combine(root ?? string.Empty, $"{x.Groups["dep"]}.js")).ToList();
        }


        [GeneratedRegex(@"//\s*using\s+(?<dep>\S+?)(\.js|\s|$)", RegexOptions.Compiled)]
        private partial Regex GetLinks();
    }
}

internal record struct Component(string FilePath, string Content, List<string> Dependencies);