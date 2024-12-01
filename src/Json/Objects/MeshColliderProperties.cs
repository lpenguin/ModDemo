using System.Text.Json.Serialization;

namespace ModDemo.Json.Objects;

public class MeshColliderProperties : ColliderProperties
{
    public MeshColliderProperties()
    {
        Type = ColliderType.Mesh;
    }

    [JsonPropertyName("mesh")]
    public string Mesh { get; set; }
}