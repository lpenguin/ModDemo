using System.Text.Json.Serialization;
using ModDemo.Json.Converter;

namespace ModDemo.Json.Objects
{
    [JsonDerivedTypeConverter("type")]
    [JsonDerivedType("prop", typeof(PropDefinition))]
    [JsonDerivedType("vehicle", typeof(VehicleDefinition))]
    [JsonDerivedType("scene", typeof(SceneDefinition))]
    [JsonDerivedType("weapon", typeof(WeaponDefinition))]
    public abstract class ObjectDefinition
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public ObjectType Type { get; set; }
        
        [JsonPropertyName("script")]
        public string? Script { get; set; }
    }
}
