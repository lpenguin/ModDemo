using System.Collections.Generic;
using System.Text.Json.Serialization;
using ModDemo.Json.Common;

namespace ModDemo.Json.Objects;

public class VehicleDefinition : ObjectDefinition
{
    [JsonPropertyName("physics")]
    public PhysicsProperties Physics { get; set; }

    [JsonPropertyName("vehicle")]
    public VehicleProperties Vehicle { get; set; }

    [JsonPropertyName("mesh")]
    public MeshProperties Mesh { get; set; }

    [JsonPropertyName("wheels")]
    public List<WheelProperties> Wheels { get; set; }
    
    [JsonPropertyName("weapon_slots")]
    public List<Vector3> WeaponSlots { get; set; }

}