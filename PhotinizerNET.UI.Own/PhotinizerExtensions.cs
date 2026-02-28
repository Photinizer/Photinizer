namespace PhotinizerNET.UI.Own;

public static class PhotinizerExtensions
{
    public static Photinizer AddOwnUI(this Photinizer photinizer, string pathToComponents)
    {
        photinizer.SetUI(new PhotinizerOwnUI(pathToComponents));
        return photinizer;
    }
}