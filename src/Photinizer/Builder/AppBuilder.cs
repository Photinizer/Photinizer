using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Photinizer.Exceptions;
using Photinizer.Messaging;
using Photinizer.Settings;

namespace Photinizer.Builder;

internal sealed class AppBuilder : IAppBuilder, ILoggingBuilder
{
    private readonly HostApplicationBuilder _builder;
    private readonly PhotinizerBuildOptions _buildOptions;
    private IPhotinizerUI? _ui;

    internal AppBuilder(AppOptions appOptions)
    {
        ArgumentNullException.ThrowIfNull(appOptions);
        _builder = Host.CreateApplicationBuilder(settings: new()
        {
            ApplicationName = appOptions.ApplicationName,
            ContentRootPath = appOptions.ContentRootPath,
            EnvironmentName = appOptions.EnvironmentName,
            Args = appOptions.Args
        });

        _buildOptions = new PhotinizerBuildOptions(appOptions.Args is { Length: > 0 }
            ? appOptions.Args : [.. System.Environment.GetCommandLineArgs().Skip(1)]);

        InitializeDefaults(appOptions, Configuration, Environment);

        ApplyDefaultAppConfiguration(Environment, Configuration, appOptions.Args);
        AddDefaultServices(Configuration, _builder.Services);

        Logging = this;

        //Services.AddSingleton(_ => Environment);
        //Services.AddSingleton<IConfiguration>(_ => Configuration);
        //Services.AddOptions();
        //Services.AddLogging();
    }

    public bool IsBuildMode => _buildOptions.IsBuildMode;

    ///<inheritdoc />
    public IHostEnvironment Environment => _builder.Environment;

    ///<inheritdoc cref="IAppBuilder.Services" />
    public IServiceCollection Services => _builder.Services;

    ///<inheritdoc cref="IAppBuilder.Configuration"/>
    public ConfigurationManager Configuration => _builder.Configuration;

    IConfigurationManager IAppBuilder.Configuration => Configuration;

    ///<inheritdoc />
    public ILoggingBuilder Logging { get; }

    public void UseUI(IPhotinizerUI ui) => _ui = ui;

    private static void InitializeDefaults(AppOptions appOptions, ConfigurationManager configuration, IHostEnvironment environment)
    {
        configuration[ConfigurationDefaults.ApplicationKey] = appOptions.ApplicationName
            ?? Assembly.GetEntryAssembly()?.GetName().Name;

        configuration[ConfigurationDefaults.EnvironmentKey] = appOptions.EnvironmentName
            ?? configuration[ConfigurationDefaults.EnvironmentKey] ?? Environments.Production;

        configuration[ConfigurationDefaults.ContentRootKey] = appOptions.ContentRootPath
            ?? "Frontend";

        configuration[ConfigurationDefaults.WebRootKey] = appOptions.WebRootPath
            ?? Path.Combine(configuration[ConfigurationDefaults.ContentRootKey]!, "wwwroot");

        environment.ApplicationName = configuration[ConfigurationDefaults.ApplicationKey]!;
        environment.EnvironmentName = configuration[ConfigurationDefaults.EnvironmentKey]!;
        environment.ContentRootPath = configuration[ConfigurationDefaults.ContentRootKey]!;
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

    private static void ApplyDefaultAppConfiguration(IHostEnvironment env, ConfigurationManager configuration, string[]? args)
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
        services.AddSingleton<IMessageSerializer, MessageSerializer>();
        services.AddSingleton<IMessenger, Messenger>();
    }

    public Application Build()
    {
        if (_buildOptions.IsBuildMode)
        {
            _ = _ui ?? throw new PhotinizerException("You must choose and set UI");
            var settings = Configuration.GetSection("Photinizer").Get<PhotinizerConfiguration>();
            _ui.Build(settings ?? new(), _buildOptions);
            System.Environment.Exit(0);
            return null;
        }
        Services.Configure<PhotinizerConfiguration>(Configuration.GetSection("Photinizer"));
        Services.Configure<ServiceProviderOptions>(options =>
        {
            options.ValidateScopes = Environment.IsDevelopment();
            options.ValidateOnBuild = Environment.IsDevelopment();
        });
        Services.TryAddSingleton<Application, Application>();

        var host = _builder.Build();
        var app = host.Services.GetRequiredService<Application>();
        app.Setup(host, Configuration, Environment);
        return app;
    }
}