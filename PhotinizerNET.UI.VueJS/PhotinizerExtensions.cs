namespace PhotinizerNET.UI.VueJS;

public static class PhotinizerExtensions
{
    public static Photinizer AddVueJs(this Photinizer photinizer)
    {
        photinizer.SetUI(new PhotinizerVueUI());
        return photinizer;
    }
}