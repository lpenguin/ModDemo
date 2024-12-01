using System.Text.Json.Serialization;

namespace ModDemo.Json.Objects
{
    [JsonConverter(typeof(ObjectDefinitionConverter))]
    public abstract class ObjectDefinition
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public ObjectType Type { get; set; }
    }
}
