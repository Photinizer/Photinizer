namespace Photinizer.Abstractions;

public interface IRunnableService
{
    Task StartAsync(CancellationToken cancellationToken);
}