using System.Text.Json.Serialization;

namespace ModDemo.Json.Objects;

public class RenderProperties
{
    [JsonPropertyName("mesh")]
    public string Mesh { get; set; }

    [JsonPropertyName("texture")]
    public string? Texture { get; set; }
}