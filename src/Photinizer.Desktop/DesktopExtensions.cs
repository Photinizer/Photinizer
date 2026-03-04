using Photinizer.Builder;
using Photinizer.UI.Own;
using Photinizer.UI.VueJS;

namespace Photinizer.Desktop;

public static class DesktopExtensions
{
    public static IAppBuilder UseOwnUI(this IAppBuilder builder, string? pathToComponents = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddOwnUI(pathToComponents);
    }

    public static IAppBuilder UseVueJs(this IAppBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddVueJs();
    }
}
