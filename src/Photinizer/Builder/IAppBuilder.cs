namespace Photinizer.Builder;

public interface IAppBuilder
{
    void UseUI(IPhotinizerUI ui);

    void UseServices(IServiceProvider services);

    Application Build();
}
