using Photinizer;
using Photinizer.Template.Default.Backend.Controllers;
using Photinizer.Template.Default.Backend.Services;
using Photinizer.UI.Own;

new PhotinizerHost()
    .AddOwnUI()
    .Run(config: o =>
    {
        // for example
        o.Window.SetDevToolsEnabled(true);

        o.Messenger
            .OnQuery("Hello, backend!", _ => "Hello, frontend!")
            .Register(new UserController())
            .Register(new TimeSender());
    });