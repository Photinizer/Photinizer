namespace Photinizer.Logging;

public interface IPhotinizerLogger
{
    void Log(string message, string level = "Info", Exception ex = null);
}