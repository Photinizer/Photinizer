namespace Photinizer.UI.Own;

public static class PhotinizerAppExtensions
{
    public static PhotinizerApp AddOwnUI(this PhotinizerApp photinizer, string pathToComponents = null)
    {
        photinizer.SetUI(new PhotinizerOwnUI(pathToComponents ?? Path.Combine("Frontend", "components")));
        return photinizer;
    }
}