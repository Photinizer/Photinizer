using Photinizer.Builder;
using Photino.NET;

var builder = Application.CreateBuilder(args);
var app = builder.Build();

app.MapMessage("close-window", (window, _) =>
{
    Console.WriteLine($"Closing \"{window.Title}\".");
    window.Close();
    app.Messenger.UnregisterWindow(window);
});

int _childCount = 0;

app.MapMessage("random-window", (window, _) =>
{
    var config = app.PhotinizerConfiguration.Windows["Secondary"];

    var childWindow = new PhotinoWindow(window);
    childWindow.UseOwnSettings(config).SetTitle($"{config.Title} ({++_childCount})");

    var sourcePath = app.ResolveSourcePath(config.Source);
    childWindow.Load(sourcePath);

    app.Messenger.RegisterWindow(childWindow);
    childWindow.WaitForClose();
});

app.Run();