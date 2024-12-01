using System.Text.Json.Serialization;
using ModDemo.Json.Common;

namespace ModDemo.Json.Objects;

public class BoxColliderProperties : ColliderProperties
{
    public BoxColliderProperties()
    {
        Type = ColliderType.Box;
    }

    [JsonPropertyName("size")]
    public Vector3 Size { get; set; }
}