using Photinizer.Builder;

namespace Photinizer.UI.VueJS;

public static class PhotinizerAppExtensions
{
    public static IAppBuilder AddVueJs(this IAppBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.UseUI(new PhotinizerVueUI());
        return builder;
    }
}