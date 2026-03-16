using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Photinizer.Exceptions;

namespace Photinizer.Messaging;

[DebuggerStepThrough]
internal sealed class MessageSerializer : IMessageSerializer
{
    private static readonly JsonSerializerOptions s_deserializeOptions = new() { PropertyNameCaseInsensitive = true };

    public string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value);
    }

    public T Deserialize<T>(JsonElement el, string endpoint)
    {
        try
        {
            return el.Deserialize<T>(s_deserializeOptions)!;
        }
        catch (Exception ex)
        {
            throw new PhotinizerException($"Endpoint data error: endpoint '{endpoint}' expects data of type '{typeof(T).Name}'", ex);
        }
    }

    public T Deserialize<T>(string json, JsonTypeInfo<T> jsonTypeInfo)
    {
        return JsonSerializer.Deserialize(json, jsonTypeInfo)!;
    }
}
