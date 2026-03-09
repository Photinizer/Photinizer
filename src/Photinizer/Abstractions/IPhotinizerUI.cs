using Photinizer.Builder;
using Photinizer.Settings;

namespace Photinizer;

//Bundler
public interface IPhotinizerUI
{
    public void Build(PhotinizerConfiguration settings, PhotinizerBuildOptions buildSettings);
}