using System.Text.Json.Serialization;
using ModDemo.Json.Common;

namespace ModDemo.Json.Objects;

public class WheelProperties
{
    [JsonPropertyName("transform")]
    public Transform Transform { get; set; }

    [JsonPropertyName("use_as_traction")]
    public bool UseAsTraction { get; set; }

    [JsonPropertyName("use_as_steering")]
    public bool UseAsSteering { get; set; }

    [JsonPropertyName("mesh")]
    public MeshProperties Mesh { get; set; }
}