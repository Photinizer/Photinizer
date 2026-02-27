namespace PhotinizerNET;

static class Program
{
    [STAThread]
    static void Main(string[] args)
        => new Photinizer().Run(setup: o =>
        {
            // for example
            o.Window.SetDevToolsEnabled(true);

            o.MessageBridge
                .OnQuery("Hello, backend!", _ => "Hello, frontend!")
                .OnMessage("save username", data => File.WriteAllText("data.dat", data.GetProperty("username").ToString()))
                .OnMessage("delete username", _ => File.Delete("data.dat"))
                .OnQuery("get username", _ => File.Exists("data.dat") ? File.ReadAllText("data.dat") : "dear friend");
        });
}