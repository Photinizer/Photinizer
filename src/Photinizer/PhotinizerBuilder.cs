using Photinizer.Settings;

namespace Photinizer;

internal class PhotinizerBuilder(
    PhotinizerSettings settings, 
    PhotinizerBuildSettings buildSettings, 
    IPhotinizerUI ui)
{
    public void Build() => ui.Build(settings, buildSettings);
}