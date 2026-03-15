using System.Text.Json;
using Photino.NET;

namespace Photinizer.Messaging;

internal enum HandlerType
{
    Handler,
    StaticHandler,
    AsyncHandler,
    StaticAsyncHandler
}

internal readonly record struct MessageHandler(
    HandlerType HandlerType = HandlerType.Handler,
    Action<PhotinoWindow, JsonElement>? Handler = null,
    Action<PhotinoWindow, JsonElement, IMessageSerializer, string, object>? StaticHandler = null,
    Func<PhotinoWindow, JsonElement, Task>? AsyncHandler = null,
    Func<PhotinoWindow, JsonElement, IMessageSerializer, string, object, Task>? StaticAsyncHandler = null,
    object? State = null);

internal readonly record struct TaskHandler(
    HandlerType HandlerType = HandlerType.Handler,
    Action<PhotinoWindow, JsonElement>? Handler = null,
    Action<PhotinoWindow, JsonElement, IMessageSerializer, string, object>? StaticHandler = null,
    Func<PhotinoWindow, JsonElement, Task>? AsyncHandler = null,
    Func<PhotinoWindow, JsonElement, IMessageSerializer, string, object, Task>? StaticAsyncHandler = null,
    object? State = null);

internal readonly record struct QueryHandler(
    HandlerType HandlerType = HandlerType.Handler,
    Func<PhotinoWindow, JsonElement, object?>? Handler = null,
    Func<PhotinoWindow, JsonElement, IMessageSerializer, string, object, object?>? StaticHandler = null,
    Func<PhotinoWindow, JsonElement, Task<object?>>? AsyncHandler = null,
    Func<PhotinoWindow, JsonElement, IMessageSerializer, string, object, Task<object?>>? StaticAsyncHandler = null,
    object? State = null);