using System.Text.Json.Serialization;

namespace ModDemo.Json.Common;

public class Transform
{
    [JsonPropertyName("position")]
    public Vector3? Position { get; set; }

    [JsonPropertyName("scale")]
    public Vector3? Scale { get; set; }

    [JsonPropertyName("rotation")]
    public Vector3? Rotation { get; set; }
}