using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ModDemo.Json.Objects;

public class ObjectsContainer
{
    [JsonPropertyName("objects")]
    public List<ObjectDefinition> Objects { get; set; }
}