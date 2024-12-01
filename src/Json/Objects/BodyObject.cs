using System.Text.Json.Serialization;

namespace ModDemo.Json.Objects;

public class PropObject : ObjectDefinition
{
    [JsonPropertyName("mesh")]
    public MeshProperties Mesh { get; set; }

    [JsonPropertyName("physics")]
    public PhysicsProperties? Physics { get; set; }
}
