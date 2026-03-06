namespace Photinizer.Settings;

public class PhotinizerSettings
{
    public string Title { get; set; } = "PhotinizerApp";
    public WindowSettings Window { get; set; } = new();
    public UISettings UI { get; set; } = new();
}

public class WindowSettings
{
    public int Width { get; set; } = 800;
    public int Height { get; set; } = 900;
    public bool Center { get; set; } = true;
    public bool DevToolsWhenDebug { get; set; } = true;
    public bool DevToolsAlways { get; set; }
}

public class UISettings
{
    public string RootComponent { get; set; } = "GreetingsComponent";
}
