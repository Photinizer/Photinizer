using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Photinizer.Messaging;

public interface IMessageSerializer
{
    string Serialize<T>(T value);
    T Deserialize<T>(JsonElement el, string endpoint);

    T Deserialize<T>(string json, JsonTypeInfo<T> jsonTypeInfo);
}
