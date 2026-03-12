using Microsoft.Extensions.DependencyInjection;

namespace Photinizer.Builder;

public static class AppServicesExtensions
{
    extension(Application application)
    {
        public Application RunServicesAfterStart(Func<CancellationToken>? getCancellationToken = null)
        {
            ArgumentNullException.ThrowIfNull(application);
            return application.AfterStart(app =>
                app.RunAllServicesAsync(getCancellationToken?.Invoke() ?? CancellationToken.None));
        }
    }

    extension(IPhotinizerConfiguration config)
    {
        public Task RunAllServicesAsync(CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(config);
            var runnableServices = config.Services.GetServices<IRunnableService>();
            List<Task>? all = null;
            foreach (var service in runnableServices)
            {
                (all ??= []).Add(service.StartAsync(cancellationToken));
            }

            if (all is not null) return Task.WhenAll(all);
            return Task.CompletedTask;
        }

        public Task StopAllServicesAsync(CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(config);
            var runnableServices = config.Services.GetServices<IRunnableService>();
            List<Task>? _all = null;
            foreach (var service in runnableServices)
            {
                (_all ??= []).Add(service.StopAsync(cancellationToken));
            }

            if (_all is not null) return Task.WhenAll(_all);
            return Task.CompletedTask;
        }
    }
}