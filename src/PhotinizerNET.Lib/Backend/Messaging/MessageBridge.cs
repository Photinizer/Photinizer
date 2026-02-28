using PhotinizerNET.Lib.Backend.Messaging;
using Photino.NET;
using System.Text.Json;

namespace PhotinizerNET.Backend.Messaging;

public class MessageBridge
{
    private readonly PhotinoWindow _window;
    private readonly Dictionary<string, RequestHandler> _handlers = [];

    public MessageBridge(PhotinoWindow window)
    {
        _window = window;
        _window.RegisterWebMessageReceivedHandler(OnMessageReceived);
    }

    public MessageBridge OnMessageAsync(string command, Func<JsonElement, Task> handler)
        => AddHandler(command, new(handler, NeedResponse: false));

    public MessageBridge OnTaskAsync(string command, Func<JsonElement, Task> handler)
        => AddHandler(command, new(handler, NeedResponse: true));

    public MessageBridge OnQueryAsync(string command, Func<JsonElement, Task<object>> handler)
        => AddHandler(command, new(handler, NeedResponse: true));


    public MessageBridge OnMessage(string command, Action<JsonElement> handler)

#pragma warning disable CS1998
        => OnMessageAsync(command, async el => handler(el));

    public MessageBridge OnTask(string command, Action<JsonElement> handler)
        => OnTaskAsync(command, async el => handler(el));

    public MessageBridge OnQuery(string command, Func<JsonElement, object> handler)
        => OnQueryAsync(command, async el => handler(el));
#pragma warning restore CS1998

    public static StatusCode NoAnswer() => StatusCode.NO_ANSWER;
    public static StatusCode Ok() => StatusCode.OK;

    private MessageBridge AddHandler(string command, RequestHandler handler)
    {
        _handlers[command] = handler;
        return this;
    }

    private async void OnMessageReceived(object sender, string message)
    {
        string reqId = null;
        try
        {
            var doc = JsonDocument.Parse(message);
            reqId = doc.RootElement.GetProperty("requestId").GetString();
            var command = doc.RootElement.GetProperty("command").GetString();
            doc.RootElement.TryGetProperty("args", out var args);

            if (_handlers.TryGetValue(command, out var handler))
            {
                var result = await handler.HandleFunc(args);
                if (handler.NeedResponse)
                {
                    var json = JsonSerializer.Serialize(new { requestId = reqId, data = result });
                    _window.SendWebMessage(json);
                }
            }
        }
        catch (Exception ex)
        {
            if (reqId != null)
                _window.SendWebMessage(JsonSerializer.Serialize(new { requestId = reqId, error = ex.Message }));
        }
    }
}
