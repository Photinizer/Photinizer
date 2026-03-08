using System.Text.Json;

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
    Action<JsonElement>? Handler = null,
    Action<JsonElement, IMessageSerializer, string, object>? StaticHandler = null,
    Func<JsonElement, Task>? AsyncHandler = null,
    Func<JsonElement, IMessageSerializer, string, object, Task>? StaticAsyncHandler = null,
    object? State = null);

internal readonly record struct TaskHandler(
    HandlerType HandlerType = HandlerType.Handler,
    Action<JsonElement>? Handler = null,
    Action<JsonElement, IMessageSerializer, string, object>? StaticHandler = null,
    Func<JsonElement, Task>? AsyncHandler = null,
    Func<JsonElement, IMessageSerializer, string, object, Task>? StaticAsyncHandler = null,
    object? State = null);

internal readonly record struct QueryHandler(
    HandlerType HandlerType = HandlerType.Handler,
    Func<JsonElement, object?>? Handler = null,
    Func<JsonElement, IMessageSerializer, string, object, object?>? StaticHandler = null,
    Func<JsonElement, Task<object?>>? AsyncHandler = null,
    Func<JsonElement, IMessageSerializer, string, object, Task<object?>>? StaticAsyncHandler = null,
    object? State = null);