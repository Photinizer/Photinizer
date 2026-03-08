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
        window.SetDevToolsEnabled(true);
#else
        window.SetDevToolsEnabled(windowSettings.DevToolsEnabled);
#endif
        if (windowSettings.Center) window.Center();

        return window;
    }
}
