using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ModDemo.Json.Levels;

public class Level
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("objects")]
    public List<LevelObject> Objects { get; set; }
}