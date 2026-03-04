using Photinizer.Settings;

namespace Photinizer.Builder;

//Bundler
public interface IPhotinizerUI
{
    public void Build(PhotinizerSettings settings, PhotinizerBuildOptions buildSettings);
}