using Photinizer.Core.Settings;

namespace Photinizer.UI.VueJS;


/// <summary>
/// Mock for real VueJS UI
/// </summary>
internal class PhotinizerVueUI : IPhotinizerUI
{
    public void Build(PhotinizerSettings settings, PhotinizerBuildSettings buildSettings) 
        => throw new NotImplementedException();
}
