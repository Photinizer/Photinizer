namespace Photinizer.Settings;

public record PhotinizerSettings
{
    public string Title { get; set; } = "PhotinizerApp";
    public WindowSettings Window { get; set; } = new();
    public UISettings UI { get; set; } = new();
}

public record WindowSettings
{
    public int Width { get; set; } = 800;
    public int Height { get; set; } = 900;
    public bool Center { get; set; } = true;
    public bool DevToolsEnabled { get; set; }
}

public class UISettings
{
    public string RootComponent { get; set; } = "GreetingsComponent";
}
