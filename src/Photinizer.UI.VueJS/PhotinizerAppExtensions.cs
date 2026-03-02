namespace Photinizer.UI.VueJS;

public static class PhotinizerAppExtensions
{
    public static PhotinizerApp AddVueJs(this PhotinizerApp photinizer)
    {
        photinizer.SetUI(new PhotinizerVueUI());
        return photinizer;
    }
}