using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Photinizer.Messaging;
using Photinizer.Settings;

namespace Photinizer.Builder;

internal sealed class AppBuilder : IAppBuilder, ILoggingBuilder
{
    private readonly ServiceCollection _serviceCollection = [];

    internal AppBuilder(AppOptions appOptions)
    {
        ArgumentNullException.ThrowIfNull(appOptions);

        var configuration = new ConfigurationManager();

        ApplyDefaultAppConfiguration(appOptions, configuration, out var environmentName);
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
            EnvironmentName = environmentName,
            ContentRootPath = ResolveContentRootPath(appOptions.ContentRootPath ?? configuration[ConfigurationDefaults.ContentRootKey] ?? string.Empty, AppContext.BaseDirectory),
        };

        AddDefaultServices(configuration, _serviceCollection);

        Environment = env;
        Configuration = configuration;

        Logging = this;

        _serviceCollection.AddSingleton(_ => Environment);
        _serviceCollection.AddSingleton<IConfiguration>(_ => Configuration);
        _serviceCollection.AddOptions();
        _serviceCollection.AddLogging();
    }

    ///<inheritdoc />
    public IAppEnvironment Environment { get; }

    ///<inheritdoc cref="IAppBuilder.Services" />
    public IServiceCollection Services => _serviceCollection;

    ///<inheritdoc cref="IAppBuilder.Configuration"/>
    public ConfigurationManager Configuration { get; }

    IConfigurationManager IAppBuilder.Configuration => Configuration;

    ///<inheritdoc />
    public ILoggingBuilder Logging { get; }

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
        if (string.IsNullOrWhiteSpace(contentRootPath))
        {
            return basePath;
        }
        if (Path.IsPathRooted(contentRootPath))
        {
            return contentRootPath;
        }
        return Path.Combine(Path.GetFullPath(basePath), contentRootPath);
    }

    private static void ApplyDefaultAppConfiguration(AppOptions appOptions, ConfigurationManager configuration, out string environmentName)
    {
        bool reloadOnChange = false;
        if (configuration["reloadOnChange"] is { Length: > 0 } str)
        {
            bool.TryParse(str, out reloadOnChange);
        }
        configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: reloadOnChange);

        environmentName = appOptions.EnvironmentName ?? configuration[ConfigurationDefaults.EnvironmentKey] ?? Environments.Production;
        if (!string.IsNullOrWhiteSpace(environmentName))
        {
            configuration.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: reloadOnChange);
        }

        configuration.AddEnvironmentVariables();
        if (appOptions.Args is { Length: > 0 })
        {
            configuration.AddCommandLine(appOptions.Args);
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
        services.AddSingleton<IMessageSerializer, MessageSerializer>();
        services.AddSingleton<IMessenger, Messenger>();
    }

    /// <inheritdoc />
    public Application Build()
    {
        Services.Configure<PhotinizerConfiguration>(Configuration.GetSection("Photinizer"));
        Services.Configure<ServiceProviderOptions>(options =>
        {
            options.ValidateScopes = Environment.IsDevelopment();
            options.ValidateOnBuild = Environment.IsDevelopment();
        });
        Services.TryAddSingleton<Application, Application>();

        var provider = _serviceCollection.BuildServiceProvider();
        _serviceCollection.MakeReadOnly();

        var app = provider.GetRequiredService<Application>();
        return app;
    }
}