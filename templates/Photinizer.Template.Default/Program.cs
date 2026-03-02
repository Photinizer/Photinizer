using Photinizer;
using Photinizer.UI.Own;

new PhotinizerService()
    .AddOwnUI()
    .Run(setup: o =>
    {
        // for example
        o.Window.SetDevToolsEnabled(true);

        o.Messenger
            .OnQuery("Hello, backend!", _ => "Hello, frontend!")
            .OnTask<UserDto>("save username", data => File.WriteAllText("data.dat", data.UserName))
            .OnTask("delete username", _ => File.Delete("data.dat"))
            .OnQuery("get username", _ => File.Exists("data.dat") ? File.ReadAllText("data.dat") : "dear friend");

        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(1000);
                await o.Messenger.SendTask("update timer", DateTime.Now.ToString("HH:mm:ss"));
            }
        });
    });

public record UserDto(string UserName);
