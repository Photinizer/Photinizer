using PhotinizerNET.Backend;
using PhotinizerNET.Backend.Settings;

namespace PhotinizerNET;

public class Photinizer
{
    public void Run(Action<PhotinizedApp> setup = null)
    {
        var settings = SettingsProvider.Get();
        var buildSettings = new PhotinizerBuildSettings();
        if (buildSettings.IsBuildMode)
        {
            new PhotinizerBuilder(settings, buildSettings).Build();
        }
        else
        {
            var app = new PhotinizedApp(settings);
            setup?.Invoke(app);
            app.Run();
        }
    }
}