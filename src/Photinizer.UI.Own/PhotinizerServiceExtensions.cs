namespace Photinizer.UI.Own;

public static class PhotinizerExtensions
{
    public static PhotinizerService AddOwnUI(this PhotinizerService photinizer, string pathToComponents = null)
    {
        photinizer.SetUI(new PhotinizerOwnUI(pathToComponents ?? Path.Combine("Frontend", "components")));
        return photinizer;
    }
}