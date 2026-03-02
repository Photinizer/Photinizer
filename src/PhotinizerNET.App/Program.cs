using PhotinizerNET;
using PhotinizerNET.UI.Own;

new Photinizer()
    .AddOwnUI(Path.Combine("Frontend", "components"))
    .Run(setup: o =>
    {
        // for example
        o.Window.SetDevToolsEnabled(true);

        o.Messenger
            .OnQuery("Hello, backend!", _ => "Hello, frontend!")
            .OnTask("save username", data => File.WriteAllText("data.dat", data.GetProperty("username").ToString()))
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