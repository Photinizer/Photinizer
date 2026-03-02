namespace Photinizer.Logging;

internal class DefaultLogger : IPhotinizerLogger
{
    public void Log(string message, string level = "Info", Exception ex = null)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{level}] Photinizer: {message}");
        if (ex != null)
            Console.WriteLine(ex);
    }
}