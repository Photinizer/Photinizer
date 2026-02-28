using PhotinizerNET.Core.Settings;

namespace PhotinizerNET;

public interface IPhotinizerUI
{
    public void Build(PhotinizerSettings settings, PhotinizerBuildSettings buildSettings);
}