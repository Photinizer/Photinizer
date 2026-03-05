using Photinizer.Builder;
using Photinizer.Settings;

namespace Photinizer;

//Bundler
public interface IPhotinizerUI
{
    public void Build(PhotinizerSettings settings, PhotinizerBuildOptions buildSettings);
}