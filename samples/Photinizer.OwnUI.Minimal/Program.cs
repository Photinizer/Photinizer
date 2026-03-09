#define FLUENT_
using Microsoft.Extensions.Logging;
using Photinizer.Builder;
using Photinizer.Desktop;
using Photinizer.OwnUI.Minimal.Backend.Extensions;
using Photinizer.UI.Own;

#if !FLUENT
// Minimal API style:
var builder = Application.CreateBuilder(args);
builder.UseOwnUI();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddSampleServices();

var app = builder.Build();
if (builder.IsBuildMode) return;
app.MapQuery("Hello, backend!", _ => "Hello, frontend!");
await app.RunAllServices();
app.Run();

#else
// Fluent API Style:
Application
    .Create(b => b
        .AddOwnUI()
        .Logging.ClearProviders().AddConsole()
        .Services.AddSampleServices())
    .Run(config: o =>
    {
        o.Messenger.OnQuery("Hello, backend!", _ => "Hello, frontend!");
        // await app.RunAllServices(); /// TODO: make it work for Fluent, maybe o.UseAllServices()..
    });
#endif