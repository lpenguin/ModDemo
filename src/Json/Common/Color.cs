using System.Text.Json.Serialization;

namespace ModDemo.Json.Common;

public class Color
{
    [JsonPropertyName("r")]
    public float R { get; set; }

    [JsonPropertyName("g")]
    public float G { get; set; }

    [JsonPropertyName("b")]
    public float B { get; set; }
}