namespace Photinizer.Settings;

public record PhotinizerConfiguration
{
    public Dictionary<string, WindowConfiguration> Windows { get; set; } = [];
}

public record WindowConfiguration
{
    public string Source { get; set; } = "index.html";
    public string Title { get; set; } = "PhotinizerApp";
    public WindowSettings Window { get; set; } = new();
    public UISettings UI { get; set; } = new();
}

public record WindowSettings
{
    public bool UseOsDefaultSize { get; set; } = true;
    public int Width { get; set; } = 0;
    public int Height { get; set; } = 0;
    public bool Center { get; set; } = false;
    public bool UseOsDefaultLocation { get; set; } = true;
    public bool FileSystemAccessEnabled { get; set; } = true;
    public bool DevToolsEnabled { get; set; } = true;
}

public class UISettings
{
    public string RootComponent { get; set; } = "GreetingsComponent";
}
