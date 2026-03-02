using Photinizer.Messaging;

namespace Photinizer.Template.Default.Backend.Services;

internal class TimeSender : INeedMessenger
{
    public void IncorporateMessenger(Messenger messenger) => Task.Run(async () =>
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
}