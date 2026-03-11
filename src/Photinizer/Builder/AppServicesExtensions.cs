using Microsoft.Extensions.DependencyInjection;

namespace Photinizer.Builder;

public static class AppServicesExtensions
{
    extension(Application application)
    {
        public Application RunServicesAfterStart() =>
            application.AfterStart(async app => await app.RunAllServicesAsync());

        public Task RunAllServicesAsync()
        {
            var runnableServices = application.Services.GetServices<IRunnableService>();
            List<Task>? _all = null;
            foreach (var service in runnableServices)
            {
                (_all ??= []).Add(service.StartAsync(default));
            }

            if (_all is not null) return Task.WhenAll(_all);
            return Task.CompletedTask;
        }

        public Task StopAllServicesAsync()
        {
            var runnableServices = application.Services.GetServices<IRunnableService>();
            List<Task>? _all = null;
            foreach (var service in runnableServices)
            {
                (_all ??= []).Add(service.StopAsync(default));
            }

            if (_all is not null) return Task.WhenAll(_all);
            return Task.CompletedTask;
        }
    }
}