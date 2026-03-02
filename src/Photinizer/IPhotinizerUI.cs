using Photinizer.Core.Settings;

namespace Photinizer;

public interface IPhotinizerUI
{
    public void Build(PhotinizerSettings settings, PhotinizerBuildSettings buildSettings);
}