using PhotinizerNET.UI.Own;
//using PhotinizerNET.UI.Vue; //requires project reference to PhotinizerNET.UI.Vue project

namespace PhotinizerNET.App;

static class Program
{
    [STAThread]
    static void Main(string[] args)
        => new Photinizer()
        .AddOwnUI(Path.Combine("Frontend","components"))/*or AddVueJs() */
        .Run(setup: o =>
        {
            // for example
            o.Window.SetDevToolsEnabled(true);

            o.MessageBridge
                .OnQuery("Hello, backend!", _ => "Hello, frontend!")
                .OnTask("save username", data => File.WriteAllText("data.dat", data.GetProperty("username").ToString()))
                .OnTask("delete username", _ => File.Delete("data.dat"))
                .OnQuery("get username", _ => File.Exists("data.dat") ? File.ReadAllText("data.dat") : "dear friend");

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    await o.MessageBridge.SendTask("update timer", DateTime.Now.ToString("HH:mm:ss"));
                }
            });
        });
}