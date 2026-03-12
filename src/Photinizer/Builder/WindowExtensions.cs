using Photinizer.Settings;
using Photino.NET;

namespace Photinizer.Builder;

public static class MainWindowExtensions
{
    /// <summary>
    /// Logs a message.
    /// </summary>
    /// <param name="message">Log message</param>
    public static void Log(this PhotinoWindow window, string message)
    {
        ArgumentNullException.ThrowIfNull(window);
        if (window.LogVerbosity < 1) return;
        Console.WriteLine($"Photinizer: \"{window.Title ?? "PhotinoWindow"}\"{message}");
    }

    public static PhotinoWindow UseOwnSettings(this PhotinoWindow window, WindowConfiguration settings)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(settings);

        var windowSettings = settings.Window;

        window
            .SetTitle(settings.Title)
            .SetUseOsDefaultSize(windowSettings.UseOsDefaultSize)
            .SetSize(windowSettings.Width, windowSettings.Height)
            .SetUseOsDefaultLocation(windowSettings.UseOsDefaultLocation)
            .SetFileSystemAccessEnabled(windowSettings.FileSystemAccessEnabled);
#if DEBUG
        window.SetDevToolsEnabled(true);
#else
        window.SetDevToolsEnabled(windowSettings.DevToolsEnabled);
#endif
        if (windowSettings.Center) window.Center();

        return window;
    }
}
