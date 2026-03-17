using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Photinizer.Builder;
using Photinizer.Exceptions;
using Photinizer.Messaging.Dtos;
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

    public void SendMessage(string endpoint, object parameters)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        foreach (var (_, window) in _windows)
        {
            SendRequestInternal(window, new(MessageTypes.Message, endpoint, parameters));
        }
    }

    public Task SendTask(string endpoint, object parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        if (cancellationToken.IsCancellationRequested) return Task.FromCanceled(cancellationToken);

        List<Task>? tasks = null;
        foreach (var (_, window) in _windows)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                (tasks ??= []).Add(Task.FromCanceled(cancellationToken));
                continue;
            }
            var request = new RequestDto(MessageTypes.Task, endpoint, parameters);

            var tcs = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
            var task = tcs.Task;

            if (cancellationToken.CanBeCanceled)
            {
                var registration = cancellationToken.UnsafeRegister(RegisterCallback, (this, tcs, request.Id));
                task.ContinueWith(_ => registration.Dispose(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }

            (tasks ??= []).Add(task);
            _pendingRequests[request.Id] = tcs;

            SendRequestInternal(window, request);
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

    public Task<JsonElement[]> SendQuery(string endpoint, object parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        if (cancellationToken.IsCancellationRequested) return Task.FromCanceled<JsonElement[]>(cancellationToken);

        List<Task<JsonElement>>? tasks = null;
        foreach (var (_, window) in _windows)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                (tasks ??= []).Add(Task.FromCanceled<JsonElement>(cancellationToken));
                continue;
            }
            var request = new RequestDto(MessageTypes.Query, endpoint, parameters);

            var tcs = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
            var task = tcs.Task;

            if (cancellationToken.CanBeCanceled)
            {
                var registration = cancellationToken.UnsafeRegister(RegisterCallback, (this, tcs, request.Id));
                task.ContinueWith(_ => registration.Dispose(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }

            (tasks ??= []).Add(tcs.Task);
            _pendingRequests[request.Id] = tcs;

            SendRequestInternal(window, request);
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

        try
        {
            var messageDto = _serializer.Deserialize(message, DtosJsonContext.Default.MessageDto);
            if (messageDto.Request is not null)
                await HandleRequest(window, messageDto.Request);
            else if (messageDto.Response is not null)
                HandleResponse(window, messageDto.Response);
            else if (messageDto.Error is not null)
                HandleError(window, messageDto.Error);
        }
        catch (Exception ex)
        {
            SendErrorInternal(window, new() { Error = $"Cannot process message. Error: {ex.Message}; Message: {message}"});
        }
    }
    private async Task HandleRequest(PhotinoWindow window, RequestDto request)
    {
        switch (request.Type)
        {
            case MessageTypes.Message:
                await HandleMessageAsync(window, request).ConfigureAwait(false);
                break;

            case MessageTypes.Task:
                await ExecuteTask(window, request).ConfigureAwait(false);
                SendResponseInternal(window, ResponseDto.OK(request));
                break;

            case MessageTypes.Query:
                var result = await ExecuteQuery(window, request).ConfigureAwait(false);
                SendResponseInternal(window, ResponseDto.FromResult(request, result));
                break;

            default:
                throw new InvalidOperationException($"Unsupported message type: {request.Type}");
        }
    }

    private void HandleResponse(PhotinoWindow window, ResponseDto response)
    {
        if (_pendingRequests.TryRemove(response.RequestId, out var task))
        {
            bool isSet = response.Error is not null
                ? task.TrySetException(new PhotinizerException($"Task execution error: {response.Error?.ToString() ?? "unknown"})"))
                : task.TrySetResult(response.Result!.Value);

            Debug.Assert(isSet, $"Can't set result for pending request \"{response.RequestId}\"");
        }
    }

    private void HandleError(PhotinoWindow window, ErrorNotificationDto error)
        => throw new PhotinizerException($"Frontend error: {error.Error}");



    private Task HandleMessageAsync(PhotinoWindow window, RequestDto request)
    {
        if (!_messages.TryGetValue(request.Endpoint, out var handler))
        {
            var msg = $"Can't resolve message handler for endpoint: {request.Endpoint}";
            Debug.Fail(msg);
            throw new InvalidOperationException(msg);
        }

        switch (handler.HandlerType)
        {
            case HandlerType.Handler:
                handler.Handler!(window, request.Parameters);
                return Task.CompletedTask;

            case HandlerType.StaticHandler:
                handler.StaticHandler!(window, request.Parameters, _serializer, request.Endpoint, handler.State!);
                return Task.CompletedTask;

            case HandlerType.AsyncHandler:
                return handler.AsyncHandler!(window, request.Parameters);

            case HandlerType.StaticAsyncHandler:
                return handler.StaticAsyncHandler!(window, request.Parameters, _serializer, request.Endpoint, handler.State!);

            default:
                throw new InvalidOperationException($"Unsupported handler type: {handler.HandlerType}");
        }
    }

    private Task ExecuteTask(PhotinoWindow window, RequestDto request)
    {
        if (!_tasks.TryGetValue(request.Endpoint, out var handler))
        {
            var msg = $"Can't resolve task handler for endpoint: {request.Endpoint}";
            Debug.Fail(msg);
            throw new InvalidOperationException(msg);
        }

        switch (handler.HandlerType)
        {
            case HandlerType.Handler:
                handler.Handler!(window, request.Parameters);
                return Task.CompletedTask;

            case HandlerType.StaticHandler:
                handler.StaticHandler!(window, request.Parameters, _serializer, request.Endpoint, handler.State!);
                return Task.CompletedTask;

            case HandlerType.AsyncHandler:
                return handler.AsyncHandler!(window, request.Parameters);

            case HandlerType.StaticAsyncHandler:
                return handler.StaticAsyncHandler!(window, request.Parameters, _serializer, request.Endpoint, handler.State!);

            default:
                throw new InvalidOperationException($"Unsupported handler type: {handler.HandlerType}");
        }
    }

    private Task<object?> ExecuteQuery(PhotinoWindow window, RequestDto request)
    {
        if (!_queries.TryGetValue(request.Endpoint, out var handler))
        {
            var msg = $"Can't resolve query handler for endpoint: {request.Endpoint}";
            Debug.Fail(msg);
            throw new InvalidOperationException(msg);
        }

        return handler.HandlerType switch
        {
            // TODO: maybe handlers be like Task<Window, Request, object/response> ..
            HandlerType.Handler => Task.FromResult(handler.Handler!(window, request.Parameters)),
            HandlerType.StaticHandler => Task.FromResult(handler.StaticHandler!(window, request.Parameters, _serializer, request.Endpoint, handler.State!)),
            HandlerType.AsyncHandler => handler.AsyncHandler!(window, request.Parameters),
            HandlerType.StaticAsyncHandler => handler.StaticAsyncHandler!(window, request.Parameters, _serializer, request.Endpoint, handler.State!),
            _ => throw new InvalidOperationException($"Unsupported handler type: {handler.HandlerType}")
        };
    }

    private void SendRequestInternal(PhotinoWindow window, RequestDto request) =>
    SendPackage(window, new { request });

    private void SendResponseInternal(PhotinoWindow window, ResponseDto response) =>
        SendPackage(window, new { response });

    private void SendErrorInternal(PhotinoWindow window, ErrorNotificationDto error) =>
        SendPackage(window, new { error });

    private void SendPackage(PhotinoWindow window, object package) =>
        window.SendWebMessage(_serializer.Serialize(package));
}