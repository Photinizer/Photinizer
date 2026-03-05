using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Photinizer.Messaging;
using Photino.NET;

namespace Photinizer;

public interface IPhotinizerConfiguration
{
    PhotinoWindow MainWindow { get; }
    Messenger Messenger { get; }
    IServiceProvider Services { get; }
    IConfiguration Configuration { get; }
    IAppEnvironment Environment { get; }
    ILogger Logger { get; }
}