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

        SetDefaultApplicationName(appOptions, configuration);
        SetDefaultContentRoot(appOptions, configuration);

        InitializeDefaults(appOptions, configuration);

        // Set WebRootPath if necessary
        if (appOptions.WebRootPath is not null)
        {
            configuration.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>(ConfigurationDefaults.WebRootKey, appOptions.WebRootPath),
            });
        }

        var env = new AppEnvironment()
        {
            ApplicationName = appOptions.ApplicationName ?? configuration[ConfigurationDefaults.ApplicationKey] ?? string.Empty,
            EnvironmentName = appOptions.EnvironmentName ?? configuration[ConfigurationDefaults.EnvironmentKey] ?? Environments.Production,
            ContentRootPath = ResolveContentRootPath(appOptions.ContentRootPath ?? configuration[ConfigurationDefaults.ContentRootKey] ?? string.Empty, AppContext.BaseDirectory),
        };

        ApplyDefaultAppConfiguration(env, configuration, appOptions.Args);
        AddDefaultServices(configuration, _serviceCollection);

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


    private static void SetDefaultApplicationName(AppOptions appOptions, ConfigurationManager configuration)
    {
        if (appOptions.ApplicationName is null && configuration[ConfigurationDefaults.ApplicationKey] is null)
        {
            configuration.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>(ConfigurationDefaults.ApplicationKey, Assembly.GetEntryAssembly()?.GetName().Name),
            });
        }
    }

    private static void SetDefaultContentRoot(AppOptions appOptions, ConfigurationManager configuration)
    {
        if (appOptions.ContentRootPath is null && configuration[ConfigurationDefaults.ContentRootKey] is null)
        {
            configuration.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>(ConfigurationDefaults.ContentRootKey, "Frontend"),
            });
        }
    }

    private static void InitializeDefaults(AppOptions appOptions, ConfigurationManager configuration)
    {
        // AppOptions override all other config sources.
        List<KeyValuePair<string, string?>>? optionList = null;
        if (appOptions.ApplicationName is not null)
        {
            (optionList ??= []).Add(new(ConfigurationDefaults.ApplicationKey, appOptions.ApplicationName));
        }
        if (appOptions.EnvironmentName is not null)
        {
            (optionList ??= []).Add(new(ConfigurationDefaults.EnvironmentKey, appOptions.EnvironmentName));
        }
        if (appOptions.ContentRootPath is not null)
        {
            (optionList ??= []).Add(new(ConfigurationDefaults.ContentRootKey, appOptions.ContentRootPath));
        }
        if (appOptions.WebRootPath is not null)
        {
            (optionList ??= []).Add(new(ConfigurationDefaults.WebRootKey, appOptions.WebRootPath));
        }
        if (optionList is not null)
        {
            configuration.AddInMemoryCollection(optionList);
        }
    }

    internal static string ResolveContentRootPath(string? contentRootPath, string basePath)
    {
        if (string.IsNullOrEmpty(contentRootPath))
        {
            return basePath;
        }
        if (Path.IsPathRooted(contentRootPath))
        {
            return contentRootPath;
        }
        return Path.Combine(Path.GetFullPath(basePath), contentRootPath);
    }

    private static void ApplyDefaultAppConfiguration(IAppEnvironment env, ConfigurationManager configuration, string[]? args)
    {
        bool reloadOnChange = false;
        if (configuration["reloadOnChange"] is { Length: > 0 } str)
        {
            bool.TryParse(str, out reloadOnChange);
        }
        configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: reloadOnChange)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: reloadOnChange);

        configuration.AddEnvironmentVariables();
        if (args is { Length: > 0 })
        {
            configuration.AddCommandLine(args);
        }
    }

    private static void AddDefaultServices(ConfigurationManager configuration, IServiceCollection services)
    {
        services.AddLogging(logging =>
        {
            logging.AddConfiguration(configuration.GetSection("Logging"));
            logging.AddSimpleConsole();

            logging.Configure(options =>
            {
                options.ActivityTrackingOptions =
                    ActivityTrackingOptions.SpanId |
                    ActivityTrackingOptions.TraceId |
                    ActivityTrackingOptions.ParentId;
            });
        });
    }

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