using System.Text.Json;

namespace Photinizer.Messaging;

internal readonly record struct MessageHandler(Action<JsonElement>? Handler = null, Func<JsonElement, Task>? AsyncHandler = null);
internal readonly record struct TaskHandler(Action<JsonElement>? Handler = null, Func<JsonElement, Task>? AsyncHandler = null);
internal readonly record struct QueryHandler(Func<JsonElement, object?>? Handler = null, Func<JsonElement, Task<object?>>? AsyncHandler = null);