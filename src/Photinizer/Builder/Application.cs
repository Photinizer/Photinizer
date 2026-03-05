using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Photinizer.Messaging;
using Photinizer.Settings;
using Photino.NET;

namespace Photinizer.Builder;

public class Application : IPhotinizerConfiguration
{
    private static int s_appIsCreated;
    private int _isRunning;
    private readonly PhotinizerSettings _settings = null!;

    internal Application() => IsBuildMode = true;//bundler stub

    internal Application(IServiceProvider services, PhotinizerSettings settings)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(settings);
        if (Interlocked.CompareExchange(ref s_appIsCreated, 1, 0) == 1)
        {
            throw new InvalidOperationException("Cannot create more than one Photinizer.Application instance.");
        }
        Services = services;
        _settings = settings;

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
    public IServiceProvider Services { get; } = null!;

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
    public ILogger Logger { get; } = null!;

    private bool IsBuildMode { get; }

    public bool IsRunning => Volatile.Read(ref _isRunning) == 1;

    public PhotinoWindow MainWindow
    {
        get => field ?? throw new NullReferenceException("MainWindow is not created yet."); 
        private set;
    }

    public Messenger Messenger
    {
        get => field ?? throw new NullReferenceException("Messenger is not created yet."); 
        private set;
    }

    internal Application AfterStart(Action<Application> callback)
    {
        AfterStartCallback += callback;//TODO thread-safe
        return this;
    }

    public void Run(Action<IPhotinizerConfiguration>? config = null)
    {
        if (IsBuildMode) { Console.WriteLine("IsBuildMode"); return; }

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
        Messenger = new Messenger(MainWindow);

        MainWindow.UseOwnSettings(_settings);

        AfterStartCallback?.Invoke(this);

        setup?.Invoke(this);

        MainWindow.Load(Path.Combine(Environment.ContentRootPath, "wwwroot", "index.html"));
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