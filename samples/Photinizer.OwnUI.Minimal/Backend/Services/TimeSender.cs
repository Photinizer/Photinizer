using Photinizer.Messaging;

namespace Photinizer.OwnUI.Minimal.Backend.Services;

internal class TimeSender(IMessenger messenger) : IRunnableService
{
    private Task? _backgroundTask;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_backgroundTask is not null) return Task.CompletedTask;

        _backgroundTask = Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            try
            {
                while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
                    await messenger.SendTask("update timer", DateTime.Now.ToString("HH:mm:ss")).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Time sender error: {ex.Message}");
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_backgroundTask is null) return Task.CompletedTask;
        return Task.WhenAny(_backgroundTask, Task.Delay(Timeout.Infinite, cancellationToken));
    }
}
