using Microsoft.Extensions.Logging;
using Photinizer.Builder;
using Photinizer.Desktop;
using Photinizer.OwnUI.Minimal.Backend.Extensions;

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