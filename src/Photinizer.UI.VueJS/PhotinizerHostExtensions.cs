namespace Photinizer.UI.VueJS;

public static class PhotinizerHostExtensions
{
    public static PhotinizerHost AddVueJs(this PhotinizerHost photinizer)
    {
        photinizer.SetUI(new PhotinizerVueUI());
        return photinizer;
    }
}