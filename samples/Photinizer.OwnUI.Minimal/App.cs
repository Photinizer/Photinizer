using Microsoft.Extensions.Logging;
using Photinizer.Builder;

namespace Photinizer.OwnUI.Minimal
{
    internal partial class App : Application
    {
        public App(IServiceProvider services) : base(services)
        {
            Logger.LogInformation("App ctor");
        }
    }
}
