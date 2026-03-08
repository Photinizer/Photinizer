using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Photino.NET;

namespace Photinizer.Messaging;

internal sealed class Messenger : IMessenger
{
    private readonly IMessageSerializer _serializer;
    private readonly ConcurrentDictionary<int, PhotinoWindow> _windows = new();

    private readonly ConcurrentDictionary<string, MessageHandler> _messages = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, TaskHandler> _tasks = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, QueryHandler> _queries = new(StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonElement>> _pendingRequests = new(StringComparer.OrdinalIgnoreCase);


    public Messenger(IMessageSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        _serializer = serializer;
    }

    internal static int NewId
    {
        get
        {
            int newId;
            do
            {
                newId = Interlocked.Increment(ref field);
            } while (newId == 0);
            return newId;
        }
    }

    public void RegisterWindow(PhotinoWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);

        while (!_windows.TryAdd(NewId, window))
        {
        }

        window.RegisterWebMessageReceivedHandler(OnMessageReceived);
    }

    public void UnregisterWindow(PhotinoWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);

        var pair = _windows.FirstOrDefault(p => p.Value == window);
        if (pair.Value is not null && _windows.TryRemove(pair.Key, out var w))
        {
            Debug.Assert(window == w);
            w.WebMessageReceived -= OnMessageReceived;
        }
    }

    #region Messages

    public IMessenger OnMessage(string endpoint, Action<JsonElement> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _messages[endpoint] = new MessageHandler(Handler: handler);
        return this;
    }

    public IMessenger OnMessage<T>(string endpoint, Action<T> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _messages[endpoint] = new MessageHandler(HandlerType.StaticHandler, StaticHandler: Handle, State: handler);
        return this;

        static void Handle(JsonElement el, IMessageSerializer serializer, string endpoint, object state)
        {
            var obj = serializer.Deserialize<T>(el, endpoint);
            var handler = (Action<T>)state;
            handler(obj);
        }
    }

    public IMessenger OnMessageAsync(string endpoint, Func<JsonElement, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _messages[endpoint] = new MessageHandler(HandlerType.AsyncHandler, AsyncHandler: handler);
        return this;
    }

    public IMessenger OnMessageAsync<T>(string endpoint, Func<T, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _messages[endpoint] = new MessageHandler(HandlerType.StaticAsyncHandler, StaticAsyncHandler: Handle, State: handler);
        return this;

        static Task Handle(JsonElement el, IMessageSerializer serializer, string endpoint, object state)
        {
            var obj = serializer.Deserialize<T>(el, endpoint);
            var handler = (Func<T, Task>)state;
            return handler(obj);
        }
    }

    #endregion

    #region OnTask

    public IMessenger OnTask(string endpoint, Action<JsonElement> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _tasks[endpoint] = new TaskHandler(Handler: handler);
        return this;
    }

    public IMessenger OnTask<T>(string endpoint, Action<T> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _tasks[endpoint] = new TaskHandler(HandlerType.StaticHandler, StaticHandler: Handle, State: handler);
        return this;

        static void Handle(JsonElement el, IMessageSerializer serializer, string endpoint, object state)
        {
            var obj = serializer.Deserialize<T>(el, endpoint);
            var handler = (Action<T>)state;
            handler(obj);
        }
    }

    public IMessenger OnTaskAsync(string endpoint, Func<JsonElement, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _tasks[endpoint] = new TaskHandler(HandlerType.AsyncHandler, AsyncHandler: handler);
        return this;
    }

    public IMessenger OnTaskAsync<T>(string endpoint, Func<T, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _tasks[endpoint] = new TaskHandler(HandlerType.StaticAsyncHandler, StaticAsyncHandler: Handle, State: handler);
        return this;

        static Task Handle(JsonElement el, IMessageSerializer serializer, string endpoint, object state)
        {
            var obj = serializer.Deserialize<T>(el, endpoint);
            var handler = (Func<T, Task>)state;
            return handler(obj);
        }
    }

    #endregion

    #region OnQuery

    public IMessenger OnQuery(string endpoint, Func<JsonElement, object?> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _queries[endpoint] = new QueryHandler(Handler: handler);
        return this;
    }

    public IMessenger OnQuery<T>(string endpoint, Func<T, object?> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _queries[endpoint] = new QueryHandler(HandlerType.StaticHandler, StaticHandler: Handle, State: handler);
        return this;

        static object? Handle(JsonElement el, IMessageSerializer serializer, string endpoint, object state)
        {
            var obj = serializer.Deserialize<T>(el, endpoint);
            var handler = (Func<T, object?>)state;
            return handler(obj);
        }
    }

    public IMessenger OnQueryAsync(string endpoint, Func<JsonElement, Task<object?>> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _queries[endpoint] = new QueryHandler(HandlerType.AsyncHandler, AsyncHandler: handler);
        return this;
    }

    public IMessenger OnQueryAsync<T>(string endpoint, Func<T, Task<object?>> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _queries[endpoint] = new QueryHandler(HandlerType.StaticAsyncHandler, StaticAsyncHandler: Handle, State: handler);
        return this;

        static Task<object?> Handle(JsonElement el, IMessageSerializer serializer, string endpoint, object state)
        {
            var obj = serializer.Deserialize<T>(el, endpoint);
            var handler = (Func<T, Task<object?>>)state;
            return handler(obj);
        }
    }

    #endregion

    public IMessenger Register(INeedMessenger service)
    {
        ArgumentNullException.ThrowIfNull(service);
        service.IncorporateMessenger(this);
        return this;
    }

    public static StatusCode NoAnswer() => StatusCode.NO_ANSWER;
    public static StatusCode Ok() => StatusCode.OK;

    public void SendMessage(string endpoint, object data)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        foreach (var pair in _windows)
        {
            var window = pair.Value;

            var reqId = Guid.NewGuid().ToString();
            var json = _serializer.Serialize(new { endpoint, requestId = reqId, data });

            window.SendWebMessage(json);
        }

    }

    public Task SendTask(string endpoint, object data)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        List<Task>? tasks = null;
        foreach (var pair in _windows)
        {
            var window = pair.Value;

            var reqId = Guid.NewGuid().ToString();
            var json = _serializer.Serialize(new { endpoint, requestId = reqId, data });

            var tcs = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingRequests[reqId] = tcs;
            window.SendWebMessage(json);
            (tasks ??= []).Add(tcs.Task);
        }

        if (tasks is not null)
        {
            return Task.WhenAll(tasks);
        }

        Debug.Fail("Has no tasks");
        return Task.CompletedTask;
    }

    public Task<JsonElement[]> SendQuery(string endpoint, object data)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        List<Task<JsonElement>>? tasks = null;
        foreach (var pair in _windows)
        {
            var window = pair.Value;

            var reqId = Guid.NewGuid().ToString();
            var json = _serializer.Serialize(new { endpoint, requestId = reqId, data });

            var tcs = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingRequests[reqId] = tcs;
            window.SendWebMessage(json);
            (tasks ??= []).Add(tcs.Task);
        }

        if (tasks is not null)
        {
            return Task.WhenAll(tasks);
        }

        Debug.Fail("Has no queries");
        return Task.FromResult(Array.Empty<JsonElement>());
    }

    private async void OnMessageReceived(object? sender, string message)
    {
        var window = (PhotinoWindow)sender!;

        string? reqId = null;
        try
        {
            var msg = _serializer.Deserialize(message, MessageJsonContext.Default.MessageBase);

            var endpoint = msg?.Endpoint;
            if (endpoint == null) return;

            reqId = msg?.RequestId;
            if (string.IsNullOrEmpty(reqId)) return;

            if (_pendingRequests.TryRemove(reqId, out var task))
            {
                task.SetResult(msg!.Data);
                return;
            }

            object? result;
            string? json;
            switch (msg!.Type)
            {
                case MessageType.Message:
                    await HandleMessageAsync(endpoint, msg.Data).ConfigureAwait(false);
                    break;
                case MessageType.Task:
                    await ExecuteTask(endpoint, msg.Data).ConfigureAwait(false);
                    result = StatusCode.OK;
                    json = _serializer.Serialize(new { requestId = reqId, data = result });
                    window.SendWebMessage(json);
                    break;
                case MessageType.Query:
                    result = await ExecuteQuery(endpoint, msg.Data).ConfigureAwait(false);
                    json = _serializer.Serialize(new { requestId = reqId, data = result });
                    window.SendWebMessage(json);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported message type: {msg.Type}");
            }
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrEmpty(reqId))
            {
                var json = _serializer.Serialize(new { requestId = reqId, error = ex.Message });
                window.SendWebMessage(json);
            }
        }
    }

    private Task HandleMessageAsync(string endpoint, JsonElement data)
    {
        if (!_messages.TryGetValue(endpoint, out var handler))
        {
            var msg = $"Can't resolve message handler for endpoint: {endpoint}";
            Debug.Fail(msg);
            throw new InvalidOperationException(msg);
        }

        switch (handler.HandlerType)
        {
            case HandlerType.Handler:
                handler.Handler!(data);
                return Task.CompletedTask;

            case HandlerType.StaticHandler:
                handler.StaticHandler!(data, _serializer, endpoint, handler.State!);
                return Task.CompletedTask;

            case HandlerType.AsyncHandler:
                return handler.AsyncHandler!(data);

            case HandlerType.StaticAsyncHandler:
                return handler.StaticAsyncHandler!(data, _serializer, endpoint, handler.State!);

            default:
                throw new InvalidOperationException($"Unsupported handler type: {handler.HandlerType}");
        }
    }

    private Task ExecuteTask(string endpoint, JsonElement data)
    {
        if (!_tasks.TryGetValue(endpoint, out var handler))
        {
            var msg = $"Can't resolve task handler for endpoint: {endpoint}";
            Debug.Fail(msg);
            throw new InvalidOperationException(msg);
        }

        switch (handler.HandlerType)
        {
            case HandlerType.Handler:
                handler.Handler!(data);
                return Task.CompletedTask;

            case HandlerType.StaticHandler:
                handler.StaticHandler!(data, _serializer, endpoint, handler.State!);
                return Task.CompletedTask;

            case HandlerType.AsyncHandler:
                return handler.AsyncHandler!(data);

            case HandlerType.StaticAsyncHandler:
                return handler.StaticAsyncHandler!(data, _serializer, endpoint, handler.State!);

            default:
                throw new InvalidOperationException($"Unsupported handler type: {handler.HandlerType}");
        }
    }

    private Task<object?> ExecuteQuery(string endpoint, JsonElement data)
    {
        if (!_queries.TryGetValue(endpoint, out var handler))
        {
            var msg = $"Can't resolve query handler for endpoint: {endpoint}";
            Debug.Fail(msg);
            throw new InvalidOperationException(msg);
        }

        return handler.HandlerType switch
        {
            HandlerType.Handler => Task.FromResult(handler.Handler!(data)),
            HandlerType.StaticHandler => Task.FromResult(handler.StaticHandler!(data, _serializer, endpoint, handler.State!)),
            HandlerType.AsyncHandler => handler.AsyncHandler!(data),
            HandlerType.StaticAsyncHandler => handler.StaticAsyncHandler!(data, _serializer, endpoint, handler.State!),
            _ => throw new InvalidOperationException($"Unsupported handler type: {handler.HandlerType}")
        };
    }
}
