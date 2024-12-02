using System.Text.Json.Serialization;
using ModDemo.Json.Common;
using ModDemo.Json.Objects;

namespace ModDemo.Json.Levels;

public class LevelObject
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("objectId")] 
    public string ObjectId { get; set; }

    [JsonPropertyName("transform")]
    public Transform Transform { get; set; }
    
    [JsonPropertyName("tags")]
    public string[] Tags { get; set; }
}