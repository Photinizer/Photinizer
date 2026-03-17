using System.Text.Json.Serialization;

namespace Photinizer.Messaging.Dtos;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
[JsonSerializable(typeof(MessageDto))]
internal partial class DtosJsonContext : JsonSerializerContext;