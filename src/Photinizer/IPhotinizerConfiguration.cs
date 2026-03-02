using Photinizer.Messaging;
using Photino.NET;

namespace Photinizer;

public interface IPhotinizerConfiguration
{
    PhotinoWindow Window { get; }
    Messenger Messenger { get; }
}