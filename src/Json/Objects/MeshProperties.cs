using System.Text.Json.Serialization;
using ModDemo.Json.Common;

namespace ModDemo.Json.Objects;

public class MeshProperties
{
    [JsonPropertyName("render")]
    public RenderProperties Render { get; set; }

    [JsonPropertyName("transform")]
    public Transform? Transform { get; set; }
}