using System.Collections.Generic;
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
    public Dictionary<string, string>? Tags { get; set; }
        
    public bool HasTag(string tagName)
    {
        if (Tags == null) return false;
        return Tags.ContainsKey(tagName);
    }
        
    public string GetTagValue(string tagName)
    {
        if (Tags == null) return null;
        return Tags.GetValueOrDefault(tagName);
    }
}