using PhotinizerNET.Core.Settings;
using PhotinizerNET.Exceptions;

namespace PhotinizerNET;

public class Photinizer
{
    private IPhotinizerUI? _ui;

    public void SetUI(IPhotinizerUI ui) => _ui = ui;

    public void Run(Action<PhotinizedApp>? setup = null)
    {
        var settings = SettingsProvider.Get();
        var buildSettings = new PhotinizerBuildSettings();
        if (buildSettings.IsBuildMode)
        {
            new PhotinizerBuilder(settings, buildSettings, _ui ?? throw new PhotinizerException("You must choose and set UI")).Build();
        }
        else
        {
            var app = new PhotinizedApp(settings);
            setup?.Invoke(app);
            app.Run();
        }
    }
}