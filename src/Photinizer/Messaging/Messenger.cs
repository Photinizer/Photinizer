using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Photinizer.Builder;
using Photinizer.Utilities;
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

    public void RegisterWindow(PhotinoWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);
        int id;
        do
        {
            id = IdGenerator<PhotinoWindow>.NewId;
        } while (!_windows.TryAdd(id, window));
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

    public IMessenger OnMessage(string endpoint, Action<PhotinoWindow, JsonElement> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _messages[endpoint] = new MessageHandler(Handler: handler);
        return this;
    }

    public IMessenger OnMessage<T>(string endpoint, Action<PhotinoWindow, T> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _messages[endpoint] = new MessageHandler(HandlerType.StaticHandler, StaticHandler: Handle, State: handler);
        return this;

        static void Handle(PhotinoWindow w, JsonElement el, IMessageSerializer serializer, string endpoint, object state)
        {
            var obj = serializer.Deserialize<T>(el, endpoint);
            var handler = (Action<PhotinoWindow, T>)state;
            handler(w, obj);
        }
    }

    public IMessenger OnMessageAsync(string endpoint, Func<PhotinoWindow, JsonElement, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _messages[endpoint] = new MessageHandler(HandlerType.AsyncHandler, AsyncHandler: handler);
        return this;
    }

    public IMessenger OnMessageAsync<T>(string endpoint, Func<PhotinoWindow, T, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _messages[endpoint] = new MessageHandler(HandlerType.StaticAsyncHandler, StaticAsyncHandler: Handle, State: handler);
        return this;

        static Task Handle(PhotinoWindow w, JsonElement el, IMessageSerializer serializer, string endpoint, object state)
        {
            var obj = serializer.Deserialize<T>(el, endpoint);
            var handler = (Func<PhotinoWindow, T, Task>)state;
            return handler(w, obj);
        }
    }

    #endregion

    #region OnTask

    public IMessenger OnTask(string endpoint, Action<PhotinoWindow, JsonElement> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _tasks[endpoint] = new TaskHandler(Handler: handler);
        return this;
    }

    public IMessenger OnTask<T>(string endpoint, Action<PhotinoWindow, T> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _tasks[endpoint] = new TaskHandler(HandlerType.StaticHandler, StaticHandler: Handle, State: handler);
        return this;

        static void Handle(PhotinoWindow w, JsonElement el, IMessageSerializer serializer, string endpoint, object state)
        {
            var obj = serializer.Deserialize<T>(el, endpoint);
            var handler = (Action<PhotinoWindow, T>)state;
            handler(w, obj);
        }
    }

    public IMessenger OnTaskAsync(string endpoint, Func<PhotinoWindow, JsonElement, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _tasks[endpoint] = new TaskHandler(HandlerType.AsyncHandler, AsyncHandler: handler);
        return this;
    }

    public IMessenger OnTaskAsync<T>(string endpoint, Func<PhotinoWindow, T, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _tasks[endpoint] = new TaskHandler(HandlerType.StaticAsyncHandler, StaticAsyncHandler: Handle, State: handler);
        return this;

        static Task Handle(PhotinoWindow w, JsonElement el, IMessageSerializer serializer, string endpoint, object state)
        {
            var obj = serializer.Deserialize<T>(el, endpoint);
            var handler = (Func<PhotinoWindow, T, Task>)state;
            return handler(w, obj);
        }
    }

    #endregion

    #region OnQuery

    public IMessenger OnQuery(string endpoint, Func<PhotinoWindow, JsonElement, object?> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _queries[endpoint] = new QueryHandler(Handler: handler);
        return this;
    }

    public IMessenger OnQuery<T>(string endpoint, Func<PhotinoWindow, T, object?> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _queries[endpoint] = new QueryHandler(HandlerType.StaticHandler, StaticHandler: Handle, State: handler);
        return this;

        static object? Handle(PhotinoWindow w, JsonElement el, IMessageSerializer serializer, string endpoint, object state)
        {
            var obj = serializer.Deserialize<T>(el, endpoint);
            var handler = (Func<PhotinoWindow, T, object?>)state;
            return handler(w,obj);
        }
    }

    public IMessenger OnQueryAsync(string endpoint, Func<PhotinoWindow, JsonElement, Task<object?>> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _queries[endpoint] = new QueryHandler(HandlerType.AsyncHandler, AsyncHandler: handler);
        return this;
    }

    public IMessenger OnQueryAsync<T>(string endpoint, Func<PhotinoWindow, T, Task<object?>> handler)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(handler);
        _queries[endpoint] = new QueryHandler(HandlerType.StaticAsyncHandler, StaticAsyncHandler: Handle, State: handler);
        return this;

        static Task<object?> Handle(PhotinoWindow w, JsonElement el, IMessageSerializer serializer, string endpoint, object state)
        {
            var obj = serializer.Deserialize<T>(el, endpoint);
            var handler = (Func<PhotinoWindow, T, Task<object?>>)state;
            return handler(w, obj);
        }
    }

    #endregion

    public static StatusCode NoAnswer() => StatusCode.NO_ANSWER;
    public static StatusCode Ok() => StatusCode.OK;

    public void SendMessage(string endpoint, object data)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        foreach (var pair in _windows)
        {
            var reqId = Guid.NewGuid().ToString();
            var json = _serializer.Serialize(new { endpoint, requestId = reqId, data });
            var window = pair.Value;
            window.SendWebMessage(json);
        }
    }

    public Task SendTask(string endpoint, object data, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        if (cancellationToken.IsCancellationRequested) return Task.FromCanceled(cancellationToken);

        List<Task>? tasks = null;
        foreach (var pair in _windows)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                (tasks ??= []).Add(Task.FromCanceled(cancellationToken));
                continue;
            }

            var reqId = Guid.NewGuid().ToString();

            var tcs = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
            var task = tcs.Task;

            if (cancellationToken.CanBeCanceled)
            {
                var registration = cancellationToken.UnsafeRegister(RegisterCallback, (this, tcs, reqId));
                task.ContinueWith(_ => registration.Dispose(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }

            (tasks ??= []).Add(task);
            _pendingRequests[reqId] = tcs;

            var json = _serializer.Serialize(new { endpoint, requestId = reqId, data });
            var window = pair.Value;
            window.SendWebMessage(json);
        }

        if (tasks is not null)
        {
            return Task.WhenAll(tasks);
        }

        Debug.Fail("Has no tasks");
        return Task.CompletedTask;

        static void RegisterCallback(object? state, CancellationToken ct)
        {
            var (messenger, tcs, reqId) = ((Messenger, TaskCompletionSource<JsonElement>, string))state!;

            bool isSet = tcs.TrySetCanceled(ct);
            if (!isSet)
            {
                var msg = $"Can't set canceled for task pending request \"{reqId}\"";
                Debug.Fail(msg);
                Console.WriteLine(msg);
            }

            bool result = messenger._pendingRequests.TryRemove(reqId, out var t);
            Debug.Assert(result && tcs == t);
        }
    }

    public Task<JsonElement[]> SendQuery(string endpoint, object data, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        if (cancellationToken.IsCancellationRequested) return Task.FromCanceled<JsonElement[]>(cancellationToken);

        List<Task<JsonElement>>? tasks = null;
        foreach (var pair in _windows)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                (tasks ??= []).Add(Task.FromCanceled<JsonElement>(cancellationToken));
                continue;
            }

            var reqId = Guid.NewGuid().ToString();

            var tcs = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
            var task = tcs.Task;

            if (cancellationToken.CanBeCanceled)
            {
                var registration = cancellationToken.UnsafeRegister(RegisterCallback, (this, tcs, reqId));
                task.ContinueWith(_ => registration.Dispose(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }

            (tasks ??= []).Add(tcs.Task);
            _pendingRequests[reqId] = tcs;

            var json = _serializer.Serialize(new { endpoint, requestId = reqId, data });
            var window = pair.Value;
            window.SendWebMessage(json);
        }

        if (tasks is not null)
        {
            return Task.WhenAll(tasks);
        }

        Debug.Fail("Has no queries");
        return Task.FromResult(Array.Empty<JsonElement>());

        static void RegisterCallback(object? state, CancellationToken ct)
        {
            var (messenger, tcs, reqId) = ((Messenger, TaskCompletionSource<JsonElement>, string))state!;

            bool isSet = tcs.TrySetCanceled(ct);
            if (!isSet)
            {
                var msg = $"Can't set canceled for query pending request \"{reqId}\"";
                Debug.Fail(msg);
                Console.WriteLine(msg);
            }

            bool result = messenger._pendingRequests.TryRemove(reqId, out var t);
            Debug.Assert(result && tcs == t);
        }
    }

    private async void OnMessageReceived(object? sender, string message)
    {
        var window = (PhotinoWindow)sender!;

        window.Log($".OnMessageReceived({message})");

        string? reqId = null;
        try
        {
            var msg = _serializer.Deserialize(message, MessageJsonContext.Default.MessageBase);

            var endpoint = msg?.Endpoint;
            if (endpoint == null) return;

            reqId = msg!.RequestId;
            if (string.IsNullOrEmpty(reqId) && msg.Type != MessageType.Message) return;

            if (msg.IsResponse)
            {
                if (reqId is null || !_pendingRequests.TryRemove(reqId, out var task))
                {
                    return;
                }

                bool isSet = task.TrySetResult(msg!.Data);
                Debug.Assert(isSet, $"Can't set result for pending request \"{reqId}\"");
                return;
            }

            object? result;
            string? json;
            switch (msg.Type)
            {
                case MessageType.Message:
                    await HandleMessageAsync(endpoint, window, msg.Data).ConfigureAwait(false);
                    break;
                case MessageType.Task:
                    await ExecuteTask(endpoint, window, msg.Data).ConfigureAwait(false);
                    result = StatusCode.OK;
                    json = _serializer.Serialize(new { requestId = reqId, data = result });
                    window.SendWebMessage(json);
                    break;
                case MessageType.Query:
                    result = await ExecuteQuery(endpoint, window, msg.Data).ConfigureAwait(false);
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

    private Task HandleMessageAsync(string endpoint, PhotinoWindow window, JsonElement data)
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
                handler.Handler!(window, data);
                return Task.CompletedTask;

            case HandlerType.StaticHandler:
                handler.StaticHandler!(window, data, _serializer, endpoint, handler.State!);
                return Task.CompletedTask;

            case HandlerType.AsyncHandler:
                return handler.AsyncHandler!(window, data);

            case HandlerType.StaticAsyncHandler:
                return handler.StaticAsyncHandler!(window, data, _serializer, endpoint, handler.State!);

            default:
                throw new InvalidOperationException($"Unsupported handler type: {handler.HandlerType}");
        }
    }

    private Task ExecuteTask(string endpoint, PhotinoWindow window, JsonElement data)
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
                handler.Handler!(window, data);
                return Task.CompletedTask;

            case HandlerType.StaticHandler:
                handler.StaticHandler!(window, data, _serializer, endpoint, handler.State!);
                return Task.CompletedTask;

            case HandlerType.AsyncHandler:
                return handler.AsyncHandler!(window, data);

            case HandlerType.StaticAsyncHandler:
                return handler.StaticAsyncHandler!(window, data, _serializer, endpoint, handler.State!);

            default:
                throw new InvalidOperationException($"Unsupported handler type: {handler.HandlerType}");
        }
    }

    private Task<object?> ExecuteQuery(string endpoint, PhotinoWindow window, JsonElement data)
    {
        if (!_queries.TryGetValue(endpoint, out var handler))
        {
            var msg = $"Can't resolve query handler for endpoint: {endpoint}";
            Debug.Fail(msg);
            throw new InvalidOperationException(msg);
        }

        return handler.HandlerType switch
        {
            HandlerType.Handler => Task.FromResult(handler.Handler!(window, data)),
            HandlerType.StaticHandler => Task.FromResult(handler.StaticHandler!(window, data, _serializer, endpoint, handler.State!)),
            HandlerType.AsyncHandler => handler.AsyncHandler!(window, data),
            HandlerType.StaticAsyncHandler => handler.StaticAsyncHandler!(window, data, _serializer, endpoint, handler.State!),
            _ => throw new InvalidOperationException($"Unsupported handler type: {handler.HandlerType}")
        };
    }
}
