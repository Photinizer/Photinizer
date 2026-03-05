using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Photinizer.Exceptions;
using Photinizer.Settings;

namespace Photinizer.Builder;

internal sealed class AppBuilder : IAppBuilder
{
    private readonly PhotinizerBuildOptions _buildOptions;
    private readonly ServiceCollection _serviceCollection = new();
    private IPhotinizerUI? _ui;

    internal AppBuilder(AppOptions appOptions)
    {
        ArgumentNullException.ThrowIfNull(appOptions);
        _buildOptions = new PhotinizerBuildOptions(appOptions.Args is { Length: > 0 }
            ? appOptions.Args : [.. System.Environment.GetCommandLineArgs().Skip(1)]);

        var configuration = new ConfigurationManager();

        //set options
        List<KeyValuePair<string, string?>>? optionList = null;
        if (appOptions.ApplicationName is null && configuration["applicationName"] is null)
        {
            (optionList ??= []).Add(new("applicationName", Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty));
        }
        if (appOptions.EnvironmentName is not null && configuration["environment"] is null)
        {
            (optionList ??= []).Add(new("environment", appOptions.EnvironmentName));
        }
        if (appOptions.ContentRootPath is null && configuration["contentRoot"] is null)
        {
            (optionList ??= []).Add(new("contentRoot", Path.GetFullPath("Frontend")));
        }
        if (optionList is not null)
        {
            configuration.AddInMemoryCollection(optionList);
        }

        var env = new AppEnvironment()
        {
            ApplicationName = appOptions.ApplicationName ?? configuration["applicationName"] ?? string.Empty,
            EnvironmentName = appOptions.EnvironmentName ?? configuration["environment"] ?? string.Empty,
            ContentRootPath = appOptions.ContentRootPath ?? configuration["contentRoot"] ?? string.Empty,
        };

        bool reloadOnChange = false;
        if (configuration["reloadOnChange"] is { Length: > 0 } str)
        {
            bool.TryParse(str, out reloadOnChange);
        }
        configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: reloadOnChange)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: reloadOnChange);

        configuration.AddEnvironmentVariables();
        if (appOptions.Args is { Length: > 0 })
        {
            configuration.AddCommandLine(appOptions.Args);
        }

        Environment = env;
        Configuration = configuration;

        Logging = new LoggingBuilder(Services);

        _serviceCollection.AddSingleton(_ => Environment);
        _serviceCollection.AddSingleton<IConfiguration>( _ => Configuration);
        _serviceCollection.AddOptions();
        _serviceCollection.AddLogging();
    }

    public bool IsBuildMode => _buildOptions.IsBuildMode;


    ///<inheritdoc />
    public IAppEnvironment Environment { get; }

    ///<inheritdoc />
    public IServiceCollection Services => _serviceCollection;

    /// <inheritdoc cref="IAppBuilder.Configuration"/>
    public ConfigurationManager Configuration { get; }

    IConfigurationManager IAppBuilder.Configuration => Configuration;

    ///<inheritdoc />
    public ILoggingBuilder Logging { get; }

    public void UseUI(IPhotinizerUI ui) => _ui = ui;

    private DefaultServiceProviderFactory GetServiceProviderFactory()
    {
        if (Environment.IsDevelopment())
        {
            return new DefaultServiceProviderFactory(
                new ServiceProviderOptions
                {
                    ValidateScopes = true,
                    ValidateOnBuild = true,
                });
        }

        return new DefaultServiceProviderFactory();
    }

    public Application Build()
    {
        var settings = SettingsProvider.Get();

        if (_buildOptions.IsBuildMode)
        {
            _ = _ui ?? throw new PhotinizerException("You must choose and set UI");
            _ui.Build(settings, _buildOptions);
            return new Application();//fallback
        }

        var appServices = GetServiceProviderFactory().CreateServiceProvider(_serviceCollection);
        _serviceCollection.MakeReadOnly();
        var app = new Application(appServices, settings);
        return app;
    }

    internal sealed class LoggingBuilder(IServiceCollection services) : ILoggingBuilder
    {
        public IServiceCollection Services { get; } = services;
    }
}