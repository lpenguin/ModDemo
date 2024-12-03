using System.Text.Json.Serialization;

namespace ModDemo.Json.Objects
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ObjectType
    {
        Prop,
        Vehicle,
        Scene,
        Weapon
    }
}
