using Microsoft.Extensions.DependencyInjection;
using Photinizer.OwnUI.Minimal.Backend.Controllers;
using Photinizer.OwnUI.Minimal.Backend.DataLayer;
using Photinizer.OwnUI.Minimal.Backend.Services;

namespace Photinizer.OwnUI.Minimal.Backend.Extensions;

internal static class SericeCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSampleServices() => services
            .AddSingleton(typeof(CrudRepository<>))
            .AddSingleton<IRunnableService, TimeSender>()
            .AddSingleton<IRunnableService, UserController>();
    }
}