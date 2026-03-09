using Photinizer.Abstractions;
using Photinizer.Messaging;

namespace Photinizer.OwnUI.Minimal.Backend.Services;

internal class TimeSender(IMessenger messenger) : IRunnableService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            try
            {
                using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
                while (await timer.WaitForNextTickAsync())
                    await messenger.SendTask("update timer", DateTime.Now.ToString("HH:mm:ss"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Time sender error: {ex.Message}");
            }
        });
        return Task.CompletedTask;
    }
}