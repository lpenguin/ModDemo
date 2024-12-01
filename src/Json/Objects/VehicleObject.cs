using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ModDemo.Json.Objects;

public class VehicleObject : ObjectDefinition
{
    [JsonPropertyName("physics")]
    public PhysicsProperties Physics { get; set; }

    [JsonPropertyName("vehicle")]
    public VehicleProperties Vehicle { get; set; }

    [JsonPropertyName("mesh")]
    public MeshProperties Mesh { get; set; }

    [JsonPropertyName("wheels")]
    public List<WheelProperties> Wheels { get; set; }
}