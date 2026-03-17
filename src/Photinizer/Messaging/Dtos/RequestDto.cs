using System.Text.Json;

namespace Photinizer.Messaging.Dtos;

internal class RequestDto
{
    public RequestDto() { }

    public RequestDto(MessageTypes type, string endpoint, object parameters)
    {
        Endpoint = endpoint;
        Parameters = JsonSerializer.SerializeToElement(parameters);
        Type = type;
    }

    public string Id { get; set; } = Guid.NewGuid().ToString();
    public MessageTypes Type { get; set; }
    public string Endpoint { get; set; } = null!;
    public JsonElement Parameters { get; set; }
}