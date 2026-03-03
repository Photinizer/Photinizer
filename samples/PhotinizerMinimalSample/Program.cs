using Photinizer.Builder;
using Photinizer.Desktop;
using Photinizer.Template.Default.Backend.Controllers;
using Photinizer.Template.Default.Backend.Services;

var builder = Application.CreateBuilder(args);
builder.UseOwnUI();
var app = builder.Build();
app.MapQuery("Hello, backend!", _ => "Hello, frontend!");
app.Register(new UserController());
app.Register(new TimeSender());
app.Run();