using Photinizer.Builder;

namespace Photinizer.UI.Own;

public static class PhotinizerAppExtensions
{
    public static IAppBuilder AddOwnUI(this IAppBuilder builder, string? pathToComponents = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.UseUI(new PhotinizerOwnUI(pathToComponents ?? Path.Combine("Frontend", "components")));
        return builder;
    }
}