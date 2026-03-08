using System.Text.Json.Serialization;

namespace Photinizer.Messaging
{
    [JsonSourceGenerationOptions(
        WriteIndented = true,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
    )]
    [JsonSerializable(typeof(MessageBase))]
    internal partial class MessageJsonContext : JsonSerializerContext
    {
    }
}
