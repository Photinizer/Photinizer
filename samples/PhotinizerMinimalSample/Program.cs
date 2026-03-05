using Microsoft.Extensions.Logging;
using Photinizer.Builder;
using Photinizer.Desktop;
using Photinizer.Template.Default.Backend.Controllers;
using Photinizer.Template.Default.Backend.Services;

var builder = Application.CreateBuilder(args);
builder.UseOwnUI();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
var app = builder.Build();
if (builder.IsBuildMode) return;
app.MapQuery("Hello, backend!", _ => "Hello, frontend!");
app.Register(new UserController());
app.Register(new TimeSender());
app.Run();