using Microsoft.Extensions.Logging;
using Photinizer.Builder;

var builder = Application.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
var app = builder.Build();
app.MapQuery("Hello, backend!", _ => "Hello, frontend!");
app.Run();