using System.Text.Json.Serialization;
using ModDemo.Json.Common;

namespace ModDemo.Json.Objects;

public enum ColliderType
{
    Box,
    Mesh
}

[JsonConverter(typeof(ColliderPropertiesConverter))]
public abstract class ColliderProperties
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ColliderType Type { get; set; }

    [JsonPropertyName("transform")]
    public Transform? Transform { get; set; }
}