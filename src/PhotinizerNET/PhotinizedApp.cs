using PhotinizerNET.Core.Settings;
using PhotinizerNET.Messaging;
using Photino.NET;

namespace PhotinizerNET;

public class PhotinizedApp(PhotinizerSettings settings)
{
    public PhotinoWindow Window => field ??= CreateWindow();
    public Messenger Messenger => field ??= new(Window);

    private PhotinoWindow CreateWindow()
    {
        var windowSettings = settings.Window;

        var window = new PhotinoWindow()
            .SetTitle(settings.Title)
            .SetUseOsDefaultSize(false)
            .SetSize(windowSettings.Width, windowSettings.Height)
            .SetFileSystemAccessEnabled(false);
#if DEBUG
        if (windowSettings is { DevToolsAlways: false, DevToolsWhenDebug: true })
            window.SetDevToolsEnabled(true);
#endif
        if (windowSettings.DevToolsAlways)
            window.SetDevToolsEnabled(true);
        if (windowSettings.Center) window.Center();

        return window;
    }

    public void Run()
    {
        Window.Load(Path.Combine("Frontend", "wwwroot", "index.html"));
        Window.WaitForClose();
    }
}
