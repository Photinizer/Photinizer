namespace Photinizer.Builder;

public static class AppExtensions
{
    extension(Application)
    {
        public static Application Create(string[]? args = null) =>
            CreateBuilder(args).Build();

        public static Application Create(Action<IAppBuilder> configure, string[]? args = null)
        {
            ArgumentNullException.ThrowIfNull(configure);
            var builder = CreateBuilder(args);
            configure(builder);
            return builder.Build();
        }

        public static IAppBuilder CreateBuilder(string[]? args = null) => 
            new AppBuilder(new ApplicationOptions(){ Args = args ?? [] });
    }
}