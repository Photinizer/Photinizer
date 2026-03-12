using Microsoft.Extensions.Logging;
using Photinizer.Builder;
using Photinizer.Desktop;

var builder = Application.CreateBuilder(args);
builder.UseOwnUI();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
var app = builder.Build();
if (builder.IsBuildMode) return;
app.MapQuery("Hello, backend!", _ => "Hello, frontend!");
app.Run();