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

        Current = this;
    }

    private Action<Application>? AfterStartCallback { get; set; }

    public static Application Current { get; private set; } = null!;

    private bool IsBuildMode { get; }

    public bool IsRunning => Volatile.Read(ref _isRunning) == 1;

    public IServiceProvider Services { get; } = null!;

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
        if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1) { Console.WriteLine("Already running"); return; }

        MainWindow = new PhotinoWindow();
        Messenger = new Messenger(MainWindow);

        MainWindow.UseOwnSettings(_settings);

        AfterStartCallback?.Invoke(this);

        setup?.Invoke(this);

        MainWindow.Load(Path.Combine("Frontend", "wwwroot", "index.html"));
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