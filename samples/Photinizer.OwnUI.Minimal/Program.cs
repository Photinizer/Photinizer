#define FLUENT
using Microsoft.Extensions.Logging;
using Photinizer.Builder;
#if !FLUENT
using Photinizer.Desktop;
#else
using Photinizer.UI.Own;
#endif
using Photinizer.OwnUI.Minimal.Backend.Extensions;

using var cts = new CancellationTokenSource();

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
app.RunServicesAfterStart(() => cts.Token);
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
        o.RunAllServicesAsync(cts.Token);
        o.Messenger.OnQuery("Hello, backend!", _ => "Hello, frontend!");
    });
#endif

cts.Cancel();