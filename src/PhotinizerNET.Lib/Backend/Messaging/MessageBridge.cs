using Photino.NET;
using System.Text.Json;

namespace PhotinizerNET.Backend.Messaging;

public class MessageBridge
{
    public static Guid NO_ANSWER = Guid.NewGuid();
    public static Guid OK_200 = Guid.NewGuid();

    private readonly PhotinoWindow _window;
    private readonly Dictionary<string, Func<JsonElement, Task<object>>> _handlers = [];

    public MessageBridge(PhotinoWindow window)
    {
        _window = window;
        _window.RegisterWebMessageReceivedHandler(OnMessageReceived);
    }

    public MessageBridge OnMessageAsync(string command, Func<JsonElement, Task> handler)
        => OnThis(() => _handlers[command] = async el => { await handler(el); return NO_ANSWER; });

    public MessageBridge OnTaskAsync(string command, Func<JsonElement, Task> handler)
        => OnThis(() => _handlers[command] = async el => { await handler(el); return OK_200; });

    public MessageBridge OnQueryAsync(string command, Func<JsonElement, Task<object>> handler)
        => OnThis(() => _handlers[command] = handler);

#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
    public MessageBridge OnMessage(string command, Action<JsonElement> handler)
        => OnMessageAsync(command, async el => handler(el));

    public MessageBridge OnTask(string command, Action<JsonElement> handler)
        => OnTaskAsync(command, async el => handler(el));

    public MessageBridge OnQuery(string command, Func<JsonElement, object> handler)
        => OnQueryAsync(command, async el => handler(el));

#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод

    // TODO: think about it
    public void Call(string eventName, object data)
    {
        var json = JsonSerializer.Serialize(new { @event = eventName, data });
        _window.SendWebMessage(json);
    }

    public static Guid NoAnswer() => NO_ANSWER;
    public static Guid Ok() => OK_200;

    private MessageBridge OnThis(Action action)
    {
        action();
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
                var result = await handler(args);
                if (!result.Equals(NO_ANSWER))
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
