using System.Text.Json;
using Photinizer.Exceptions;
using Photino.NET;
namespace Photinizer.Messaging;

public class Messenger
{
    private readonly PhotinoWindow _window;
    private readonly Dictionary<string, RequestHandler> _handlers = [];
    private readonly Dictionary<string, TaskCompletionSource<JsonElement>> _pendingRequests = [];
    private static readonly JsonSerializerOptions s_deserializeOptions = new() { PropertyNameCaseInsensitive = true };

    public Messenger(PhotinoWindow window)
    {
        _window = window;
        _window.RegisterWebMessageReceivedHandler(OnMessageReceived);
    }

    #region OnMessage

    public Messenger OnMessageAsync(string endpoint, Func<JsonElement, Task> handler)
        => AddHandler(endpoint, new(handler, NeedResponse: false));

    public Messenger OnMessageAsync<T>(string endpoint, Func<T?, Task> handler)
        => AddHandler(endpoint, new(el => handler(Deserialize<T>(el, endpoint)), NeedResponse: false));

    public Messenger OnMessage(string endpoint, Action<JsonElement> handler)
    => OnMessageAsync(endpoint, el => {
        handler(el);
        return Task.CompletedTask;
    });

    public Messenger OnMessage<T>(string endpoint, Action<T?> handler)
    => OnMessageAsync(endpoint, el => {
        handler(Deserialize<T>(el, endpoint));
        return Task.CompletedTask;
    });

    #endregion

    #region OnTask
    public Messenger OnTaskAsync(string endpoint, Func<JsonElement, Task> handler)
        => AddHandler(endpoint, new(handler, NeedResponse: true));

    public Messenger OnTaskAsync<T>(string endpoint, Func<T?, Task> handler)
        => AddHandler(endpoint, new(el => handler(Deserialize<T>(el, endpoint)), NeedResponse: true));

    public Messenger OnTask(string endpoint, Action<JsonElement> handler)
        => OnTaskAsync(endpoint, el => {
            handler(el);
            return Task.CompletedTask;
        });

    public Messenger OnTask<T>(string endpoint, Action<T?> handler)
        => OnTaskAsync(endpoint, el => {
            handler(Deserialize<T>(el, endpoint));
            return Task.CompletedTask;
        });

    #endregion

    #region OnQuery
    public Messenger OnQueryAsync(string endpoint, Func<JsonElement, Task<object>> handler)
        => AddHandler(endpoint, new(handler, NeedResponse: true));

    public Messenger OnQueryAsync<T>(string endpoint, Func<T?, Task<object>> handler)
        => AddHandler(endpoint, new(el => handler(Deserialize<T>(el, endpoint)), NeedResponse: true));

    public Messenger OnQuery(string endpoint, Func<JsonElement, object> handler)
        => OnQueryAsync(endpoint, el => Task.FromResult(handler(el)));

    public Messenger OnQuery<T>(string endpoint, Func<T?, object> handler)
        => OnQueryAsync(endpoint, el => Task.FromResult(handler(Deserialize<T>(el, endpoint))));
    #endregion

    public Messenger Register(INeedMessenger service)
    {
        service.IncorporateMessenger(this);
        return this;
    }


    public static StatusCode NoAnswer() => StatusCode.NO_ANSWER;
    public static StatusCode Ok() => StatusCode.OK;

    private Messenger AddHandler(string endpoint, RequestHandler handler)
    {
        _handlers[endpoint] = handler;
        return this;
    }

    public async void SendMessage(string endpoint, object data)
        => _window.SendWebMessage(JsonSerializer.Serialize(new { endpoint, requestId = Guid.NewGuid().ToString(), data }));

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

    private static T? Deserialize<T>(JsonElement el, string endpoint)
    {
        try
        {
            return el.Deserialize<T>(s_deserializeOptions);
        }
        catch (Exception ex)
        {
            throw new PhotinizerException($"Endpoint data error: endpoint '{endpoint}' expects data of type '{typeof(T).Name}'", ex);
        }
    }


    private async void OnMessageReceived(object? sender, string message)
    {
        string? reqId = null;
        try
        {
            var doc = JsonDocument.Parse(message);
            reqId = doc.RootElement.GetProperty("requestId").GetString();
            if (string.IsNullOrEmpty(reqId)) return;

            var endpoint = doc.RootElement.GetProperty("endpoint").GetString();
            if (endpoint == null) return;

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
            if (!string.IsNullOrEmpty(reqId))
            {
                _window.SendWebMessage(JsonSerializer.Serialize(new { requestId = reqId, error = ex.Message }));
            }
        }
    }
}
