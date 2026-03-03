using Photinizer.Settings;
using Photino.NET;

namespace Photinizer.Builder;

internal static class MainWindowExtensions
{
    public static PhotinoWindow UseOwnSettings(this PhotinoWindow window, PhotinizerSettings settings)//TODO WindowSettings?
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(settings);

        var windowSettings = settings.Window;

        window
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
}
