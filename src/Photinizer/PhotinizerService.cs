using Photinizer.Core.Settings;
using Photinizer.Exceptions;
using System.Runtime.InteropServices;

namespace Photinizer;

public class PhotinizerService
{
    private IPhotinizerUI _ui;

    public void SetUI(IPhotinizerUI ui) => _ui = ui;

    public void Run(Action<PhotinizedApp> setup = null)
    {
        var settings = SettingsProvider.Get();
        var buildSettings = new PhotinizerBuildSettings();
        if (buildSettings.IsBuildMode)
        {
            new PhotinizerBuilder(settings, buildSettings, _ui ?? throw new PhotinizerException("You must choose and set UI")).Build();
        }
        else
        {

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                RunIsSTAThread(() => RunApp(settings, setup));
            else
                RunApp(settings, setup);
        }
    }

    private void RunApp(PhotinizerSettings settings, Action<PhotinizedApp> setup = null)
    {
        var app = new PhotinizedApp(settings);
        setup?.Invoke(app);
        app.Run();
    }

    private void RunIsSTAThread(ThreadStart threadStart)
    {
        Thread newThread = new(threadStart);
#pragma warning disable CA1416 // Only for Windows (already checked above)
        newThread.SetApartmentState(ApartmentState.STA);
#pragma warning restore CA1416 // Only for Windows (already checked above)
        newThread.Start();
        newThread.Join();
    }
}