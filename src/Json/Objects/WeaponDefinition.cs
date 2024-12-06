using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModDemo.Json.Common;
using ModDemo.Json.Converter;

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

        [JsonPropertyName("numProjectiles")]
        public int NumProjectiles { get; set; } = 1;
        
        [JsonPropertyName("shootDelay")]
        public float ShootDelay { get; set; }
    }

    [JsonDerivedTypeConverter("type")]
    [JsonDerivedType("bullet", typeof(BulletProjectileProperties))]
    public abstract class ProjectileProperties
    {
        [JsonPropertyName("type")]
        public ProjectileType Type { get; set; }

        [JsonPropertyName("damage")]
        public float Damage { get; set; }
        
        [JsonPropertyName("transform")]
        public Transform? Transform { get; set; }
        }

    public class BulletProjectileProperties : ProjectileProperties
    {
        [JsonPropertyName("color")]
        public Color Color { get; set; } = new Color();
        
        [JsonPropertyName("spread")]
        public float Spread { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProjectileType
    {
        Bullet,
    }
}