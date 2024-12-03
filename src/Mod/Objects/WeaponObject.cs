using Godot;
using ModDemo.Json.Objects;

namespace ModDemo.Mod.Objects;

public partial class WeaponObject : Node3D
{
    // Projectile properties exposed as separate fields
    [Export]
    public ProjectileType ProjectileType { get; set; }

    [Export]
    public float ProjectileDamage { get; set; }

    [Export]
    public Color ProjectileColor { get; set; }
}