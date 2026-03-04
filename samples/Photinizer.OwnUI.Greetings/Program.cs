using Photinizer.Builder;
using Photinizer.Template.Default.Backend.Controllers;
using Photinizer.Template.Default.Backend.Services;
using Photinizer.UI.Own;

Application
    .Create(b => b.AddOwnUI())
    .Run(config: o =>
    {
        // for example
        o.MainWindow.SetDevToolsEnabled(true);

        o.Messenger
            .OnQuery("Hello, backend!", _ => "Hello, frontend!")
            .Register(new UserController())
            .Register(new TimeSender());
    });