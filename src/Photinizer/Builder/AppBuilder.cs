using System.ComponentModel.Design;
using Photinizer.Exceptions;
using Photinizer.Settings;

namespace Photinizer.Builder;

internal sealed class AppBuilder : IAppBuilder
{
    private readonly ApplicationOptions _appOptions;
    private IPhotinizerUI? _ui;
    private IServiceProvider? _appServices;

    internal AppBuilder(ApplicationOptions appOptions)
    {
        _appOptions = appOptions ?? throw new ArgumentNullException(nameof(appOptions));
    }

    public void UseUI(IPhotinizerUI ui) => _ui = ui;

    public void UseServices(IServiceProvider services) => _appServices = services;

    public Application Build()
    {
        var settings = SettingsProvider.Get();
        var buildSettings = new PhotinizerBuildOptions(_appOptions.Args);
        if (buildSettings.IsBuildMode)
        {
            _ = _ui ?? throw new PhotinizerException("You must choose and set UI");
            _ui.Build(settings, buildSettings);
            return new Application();//fallback
        }

        var app = new Application(_appServices ?? CreateDefaultServiceContainer(), settings);
        return app;

        static IServiceProvider CreateDefaultServiceContainer() => new ServiceContainer();
    }
}