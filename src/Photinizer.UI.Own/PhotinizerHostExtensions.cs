namespace Photinizer.UI.Own;

public static class PhotinizerHostExtensions
{
    public static PhotinizerHost AddOwnUI(this PhotinizerHost photinizer, string pathToComponents = null)
    {
        photinizer.SetUI(new PhotinizerOwnUI(pathToComponents ?? Path.Combine("Frontend", "components")));
        return photinizer;
    }
}