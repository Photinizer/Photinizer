using System.Text.Json;

namespace Photinizer.Messaging;

public enum MessageType { Message, Task, Query }

public record MessageBase
{
    public MessageBase()
    {

    }

    public MessageType Type { get; init; }
    public string Endpoint { get; init; } = null!;
    public string? RequestId { get; init; }
    public JsonElement Data { get; init; }
    public bool IsResponse { get; init; }
}
