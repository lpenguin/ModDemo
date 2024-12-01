using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModDemo.Json.Objects;

public class PhysicsTypeConverter : JsonConverter<PhysicsType>
{
    public override PhysicsType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string value = reader.GetString()?.ToLowerInvariant() ?? 
                       throw new JsonException("PhysicsType value cannot be null");
            
        return value switch
        {
            "static" => PhysicsType.Static,
            "rigid_body" => PhysicsType.RigidBody,
            _ => throw new JsonException($"Invalid PhysicsType value: {value}. Expected 'static' or 'rigid_body'.")
        };
    }

    public override void Write(Utf8JsonWriter writer, PhysicsType value, JsonSerializerOptions options)
    {
        string stringValue = value switch
        {
            PhysicsType.Static => "static",
            PhysicsType.RigidBody => "rigid_body",
            _ => throw new JsonException($"Unknown PhysicsType value: {value}")
        };
        
        writer.WriteStringValue(stringValue);
    }
}

[JsonConverter(typeof(PhysicsTypeConverter))]
public enum PhysicsType
{
    Static,
    RigidBody
}

public class PhysicsProperties
{
    [JsonPropertyName("type")] public PhysicsType Type { get; set; }

    [JsonPropertyName("collider")] public ColliderProperties Collider { get; set; }

    [JsonPropertyName("mass")] public float Mass { get; set; } = 1;
}