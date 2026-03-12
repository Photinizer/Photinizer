using Microsoft.Extensions.Logging;
using Photinizer.Builder;

namespace Photinizer.OwnUI.Minimal;

internal partial class App : Application
{
    public override void Initialize() => Logger.LogInformation("App initialize");
}