using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Photinizer.Builder
{
    public static class AppBuilderExtensions
    {
        public static IAppBuilder UseApp<TApp>(this IAppBuilder builder) where TApp : Application
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Services.TryAddSingleton<Application, TApp>();
            return builder;
        }
    }
}