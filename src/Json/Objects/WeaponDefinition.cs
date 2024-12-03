using System.Text.Json.Serialization;
using ModDemo.Json.Common;

namespace ModDemo.Json.Objects
{
    public class WeaponDefinition : ObjectDefinition
    {
        [JsonPropertyName("mesh")]
        public MeshProperties Mesh { get; set; }

        [JsonPropertyName("physics")]
        public PhysicsProperties Physics { get; set; }

        [JsonPropertyName("projectile")]
        public ProjectileProperties Projectile { get; set; }
    }

    public class ProjectileProperties
    {
        [JsonPropertyName("type")]
        public ProjectileType Type { get; set; }

        [JsonPropertyName("damage")]
        public float Damage { get; set; }

        [JsonPropertyName("color")]
        public Color Color { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProjectileType
    {
        Bullet,
    }
}