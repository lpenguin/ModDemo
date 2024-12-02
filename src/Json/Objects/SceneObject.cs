using System.Text.Json.Serialization;

namespace ModDemo.Json.Objects
{
    public class SceneObject : ObjectDefinition
    {
        [JsonPropertyName("file")]
        public string File { get; set; }
    }
}
