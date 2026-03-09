using Microsoft.Extensions.DependencyInjection;
using Photinizer.Abstractions;
using Photinizer.OwnUI.Minimal.Backend.Controllers;
using Photinizer.OwnUI.Minimal.Backend.DataLayer;
using Photinizer.OwnUI.Minimal.Backend.Services;

namespace Photinizer.OwnUI.Minimal.Backend.Extensions;

internal static class SericeCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSampleServices() => services
            .AddTransient(typeof(CrudRepository<>))
            .AddTransient<IRunnableService, TimeSender>()
            .AddTransient<IRunnableService, UserController>();
    }
}