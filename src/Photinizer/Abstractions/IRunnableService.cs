namespace Photinizer;

// TODO: Replace with the Application lifecycle methods
public interface IRunnableService
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}