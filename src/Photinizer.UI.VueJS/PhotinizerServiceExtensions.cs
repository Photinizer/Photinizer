namespace Photinizer.UI.VueJS;

public static class PhotinizerServiceExtensions
{
    public static PhotinizerService AddVueJs(this PhotinizerService photinizer)
    {
        photinizer.SetUI(new PhotinizerVueUI());
        return photinizer;
    }
}