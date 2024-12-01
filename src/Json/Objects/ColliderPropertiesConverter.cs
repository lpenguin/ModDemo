using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModDemo.Json.Objects;

public class ColliderPropertiesConverter : JsonConverter<ColliderProperties>
{
    public override ColliderProperties Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var rootElement = jsonDoc.RootElement;
        
        if (!rootElement.TryGetProperty("type", out var typeProperty))
        {
            throw new JsonException("Missing required 'type' property");
        }

        var typeStr = typeProperty.GetString();
        if (!Enum.TryParse<ColliderType>(typeStr, true, out var colliderType))
        {
            throw new JsonException($"Invalid collider type: {typeStr}");
        }

        var jsonString = rootElement.GetRawText();
        return colliderType switch
        {
            ColliderType.Box => JsonSerializer.Deserialize<BoxColliderProperties>(jsonString, options),
            ColliderType.Mesh => JsonSerializer.Deserialize<MeshColliderProperties>(jsonString, options),
            _ => throw new JsonException($"Unsupported collider type: {colliderType}")
        };
    }

    public override void Write(Utf8JsonWriter writer, ColliderProperties value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}