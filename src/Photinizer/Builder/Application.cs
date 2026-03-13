using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Photinizer.Messaging;
using Photinizer.Settings;
using Photino.NET;

namespace Photinizer.Builder;

public class Application : IPhotinizerConfiguration
{
    private static int s_appIsCreated;
    private int _isRunning;

    public Application(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (Interlocked.CompareExchange(ref s_appIsCreated, 1, 0) == 1)
        {
            throw new InvalidOperationException($"Cannot create more than one {typeof(Application).FullName} instance.");
        }
        Services = services;

        Logger = Services.GetRequiredService<ILoggerFactory>().CreateLogger(Environment.ApplicationName ?? nameof(Application));

        Current = this;
    }

    private Action<Application>? AfterStartCallback { get; set; }

    /// <summary>
    /// Gets the current application instance.
    /// </summary>
    public static Application Current { get; private set; } = null!;

    /// <summary>
    /// The application's configured services.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// The application's configured <see cref="IConfiguration"/>.
    /// </summary>
    public IConfiguration Configuration => Services.GetRequiredService<IConfiguration>();

    /// <summary>
    /// The application's configured <see cref="IAppEnvironment"/>.
    /// </summary>
    public IAppEnvironment Environment => Services.GetRequiredService<IAppEnvironment>();

    /// <summary>
    /// The default logger for the application.
    /// </summary>
    public ILogger Logger { get; }

    public bool IsRunning => Volatile.Read(ref _isRunning) == 1;

    public PhotinoWindow MainWindow
    {
        get => field ?? throw new InvalidOperationException("MainWindow is not created yet.");
        private set;
    }

    public IMessenger Messenger => Services.GetRequiredService<IMessenger>();

    internal Application AfterStart(Action<Application> callback)
    {
        AfterStartCallback += callback;//TODO thread-safe
        return this;
    }

    private string ResolveSourcePath(string source)
    {
        var webRoot = Configuration[ConfigurationDefaults.WebRootKey];
        if (!string.IsNullOrWhiteSpace((webRoot)))
        {
            return Path.Combine(webRoot, source);
        }

        return Path.Combine(Environment.ContentRootPath, "wwwroot", source);
    }

    public void Run(Action<IPhotinizerConfiguration>? config = null)
    {
        if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1) { Console.WriteLine("Already running"); return; }

        if (OperatingSystem.IsWindows())
        {
            RunUiThread(() => RunApp(config));
        }
        else
        {
            RunApp(config);
        }
    }

    private void RunApp(Action<IPhotinizerConfiguration>? setup = null)
    {
        MainWindow = new PhotinoWindow();
        Messenger.RegisterWindow(MainWindow);

        var configuration = Services.GetRequiredService<IOptions<PhotinizerConfiguration>>();

        var settings = configuration.Value.Windows["MainWindow"];
        MainWindow.UseOwnSettings(settings);

        var sourcePath = ResolveSourcePath(settings.Source);
        if (!File.Exists(sourcePath))
        {
            var msg = $"Could not find source file '{sourcePath}'";
            Debug.Fail(msg);
            throw new FileNotFoundException(msg, sourcePath);
        }
        MainWindow.Load(sourcePath);

        AfterStartCallback?.Invoke(this);
        setup?.Invoke(this);

        MainWindow.WaitForClose();
    }

    private static void RunUiThread(ThreadStart threadStart)
    {
        Thread uiThread = new(threadStart);
#pragma warning disable CA1416 // Only for Windows (already checked above)
        uiThread.SetApartmentState(ApartmentState.STA);
#pragma warning restore CA1416 // Only for Windows (already checked above)
        uiThread.Start();
        uiThread.Join();
    }
}