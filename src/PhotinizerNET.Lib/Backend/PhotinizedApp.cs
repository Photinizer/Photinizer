using PhotinizerNET.Backend.Messaging;
using PhotinizerNET.Backend.Settings;
using Photino.NET;

namespace PhotinizerNET.Backend;

public class PhotinizedApp(PhotinizerSettings settings)
{
    private PhotinoWindow _window;
    public MessageBridge _messageBridge;

    public PhotinoWindow Window => _window ??= CreateWindow();
    public MessageBridge MessageBridge => _messageBridge ??= new(Window);

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
