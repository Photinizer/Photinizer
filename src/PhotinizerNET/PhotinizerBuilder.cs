using PhotinizerNET.Core.Settings;

namespace PhotinizerNET;

internal class PhotinizerBuilder(
    PhotinizerSettings settings, 
    PhotinizerBuildSettings buildSettings, 
    IPhotinizerUI ui)
{
    public void Build() => ui.Build(settings, buildSettings);
}