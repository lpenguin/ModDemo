using System.Text.Json.Serialization;
using ModDemo.Json.Common;
using ModDemo.Json.Converter;

namespace ModDemo.Json.Objects;

public enum ColliderType
{
    Box,
    Mesh
}

[JsonDerivedTypeConverter("type")]
[JsonDerivedType("box", typeof(BoxColliderProperties))]
[JsonDerivedType("mesh", typeof(MeshColliderProperties))]
public abstract class ColliderProperties
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ColliderType Type { get; set; }

    [JsonPropertyName("transform")]
    public Transform? Transform { get; set; }
}