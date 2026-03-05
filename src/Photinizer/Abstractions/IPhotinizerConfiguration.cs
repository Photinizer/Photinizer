using Photinizer.Messaging;
using Photino.NET;

namespace Photinizer;

public interface IPhotinizerConfiguration
{
    PhotinoWindow MainWindow { get; }
    Messenger Messenger { get; }
    IServiceProvider Services { get; }
}