using static PhotinizerNET.Core.Settings.PhotinizerSettings;

namespace PhotinizerNET.Core.Settings;

public record PhotinizerSettings(
    WindowSettings Window,
    UISettings UI,
    string Title = "PhotinizerApp")
{
    public record WindowSettings(
      int Width = 800,
      int Height = 900,
      bool Center = true,
      bool DevToolsWhenDebug = true,
      bool DevToolsAlways = false);

    public record UISettings(string RootComponent = "GreetingsComponent");
}