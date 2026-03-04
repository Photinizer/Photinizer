using Photinizer.Builder;
using Photinizer.Settings;

namespace Photinizer.UI.VueJS;

/// <summary>
/// Mock for real VueJS UI
/// </summary>
internal class PhotinizerVueUI : IPhotinizerUI
{
    public void Build(PhotinizerSettings settings, PhotinizerBuildOptions buildSettings) 
        => throw new NotImplementedException();
}
