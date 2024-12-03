using System.Text.Json.Serialization;

namespace ModDemo.Json.Objects;

public class PropDefinition : ObjectDefinition
{
    [JsonPropertyName("mesh")]
    public MeshProperties Mesh { get; set; }

    [JsonPropertyName("physics")]
    public PhysicsProperties? Physics { get; set; }
}
