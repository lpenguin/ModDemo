using System.Text.Json.Serialization;

namespace ModDemo.Json.Common;

public class Vector3
{
    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("y")]
    public float Y { get; set; }

    [JsonPropertyName("z")]
    public float Z { get; set; }
}