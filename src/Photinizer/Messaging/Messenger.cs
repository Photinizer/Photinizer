using Photino.NET;
using System.Text.Json;

namespace Photinizer.Messaging;

public class Messenger
{
    private readonly PhotinoWindow _window;
    private readonly Dictionary<string, RequestHandler> _handlers = [];
    private readonly Dictionary<string, TaskCompletionSource<JsonElement>> _pendingRequests = [];
    private static readonly JsonSerializerOptions _deserializeOptions = new() { PropertyNameCaseInsensitive = true };

    public Messenger(PhotinoWindow window)
    {
        _window = window;
        _window.RegisterWebMessageReceivedHandler(OnMessageReceived);
    }

    #region OnMessage

    public Messenger OnMessageAsync(string endpoint, Func<JsonElement, Task> handler)
        => AddHandler(endpoint, new(handler, NeedResponse: false));

    public Messenger OnMessageAsync<T>(string endpoint, Func<T, Task> handler)
        => AddHandler(endpoint, new(el => handler(el.Deserialize<T>()), NeedResponse: false));

    public Messenger OnMessage(string endpoint, Action<JsonElement> handler)
    => OnMessageAsync(endpoint, el => {
        handler(el);
        return Task.CompletedTask;
    });

    public Messenger OnMessage<T>(string endpoint, Action<T> handler)
    => OnMessageAsync(endpoint, el => {
        handler(el.Deserialize<T>(_deserializeOptions));
        return Task.CompletedTask;
    });

    #endregion

    #region OnTask
    public Messenger OnTaskAsync(string endpoint, Func<JsonElement, Task> handler)
        => AddHandler(endpoint, new(handler, NeedResponse: true));

    public Messenger OnTaskAsync<T>(string endpoint, Func<T, Task> handler)
        => AddHandler(endpoint, new(el => handler(el.Deserialize<T>(_deserializeOptions)), NeedResponse: true));

    public Messenger OnTask(string endpoint, Action<JsonElement> handler)
        => OnTaskAsync(endpoint, el => {
            handler(el);
            return Task.CompletedTask;
        });

    public Messenger OnTask<T>(string endpoint, Action<T> handler)
        => OnTaskAsync(endpoint, el => {
            handler(el.Deserialize<T>(_deserializeOptions));
            return Task.CompletedTask;
        });

    #endregion

    #region OnQuery
    public Messenger OnQueryAsync(string endpoint, Func<JsonElement, Task<object>> handler)
        => AddHandler(endpoint, new(handler, NeedResponse: true));

    public Messenger OnQueryAsync<T>(string endpoint, Func<T, Task<object>> handler)
        => AddHandler(endpoint, new(el => handler(el.Deserialize<T>(_deserializeOptions)), NeedResponse: true));

    public Messenger OnQuery(string endpoint, Func<JsonElement, object> handler)
        => OnQueryAsync(endpoint, el => Task.FromResult(handler(el)));

    public Messenger OnQuery<T>(string endpoint, Func<T, object> handler)
        => OnQueryAsync(endpoint, el => Task.FromResult(handler(el.Deserialize<T>(_deserializeOptions))));
    #endregion

    public Messenger Register(INeedMessenger service)
    {
        service.Incorporate(this);
        return this;
    }


    public static StatusCode NoAnswer() => StatusCode.NO_ANSWER;
    public static StatusCode Ok() => StatusCode.OK;

    private Messenger AddHandler(string endpoint, RequestHandler handler)
    {
        _handlers[endpoint] = handler;
        return this;
    }

    private async void OnMessageReceived(object sender, string message)
    {
        string reqId = null;
        try
        {
            var doc = JsonDocument.Parse(message);
            reqId = doc.RootElement.GetProperty("requestId").GetString();
            var endpoint = doc.RootElement.GetProperty("endpoint").GetString();
            doc.RootElement.TryGetProperty("data", out var data);

            if (_handlers.TryGetValue(endpoint, out var handler))
            {
                var result = await handler.HandleFunc(data).ConfigureAwait(false);
                if (handler.NeedResponse)
                {
                    var json = JsonSerializer.Serialize(new { requestId = reqId, data = result });
                    _window.SendWebMessage(json);
                }
            }
            else if (_pendingRequests.TryGetValue(reqId, out var task))
            {
                _pendingRequests.Remove(reqId);
                task.SetResult(data);
            }
        }
        catch (Exception ex)
        {
            if (reqId != null)
                _window.SendWebMessage(JsonSerializer.Serialize(new { requestId = reqId, error = ex.Message }));
        }
    }

    public async void SendMessage(string endpoint, object data)
        => _window.SendWebMessage(JsonSerializer.Serialize(new { requestId = Guid.NewGuid().ToString(), data }));

    public Task SendTask(string endpoint, object data)
    {
        var reqId = Guid.NewGuid().ToString();
        var json = JsonSerializer.Serialize(new { endpoint, requestId = reqId, data });
        var tcs = new TaskCompletionSource<JsonElement>();
        _pendingRequests[reqId] = tcs;
        _window.SendWebMessage(json);
        return tcs.Task;
    }

    public Task<JsonElement> SendQuery(string endpoint, object data)
    {
        var reqId = Guid.NewGuid().ToString();
        var json = JsonSerializer.Serialize(new { endpoint, requestId = reqId, data });
        var tcs = new TaskCompletionSource<JsonElement>();
        _pendingRequests[reqId] = tcs;
        _window.SendWebMessage(json);
        return tcs.Task;
    }
}
