using Godot;
using ModDemo.Json.Objects;

namespace ModDemo.Game.Objects;

public partial class WeaponObject : RigidBody3D
{
    // Projectile properties exposed as separate fields
    [Export]
    public ProjectileType ProjectileType { get; set; }

    [Export]
    public float ProjectileDamage { get; set; }

    [Export]
    public Color ProjectileColor { get; set; }
    
    [Export]
    public Transform3D ProjectileOrigin { get; set; }

    [Export]
    public float ProjectileSpread { get; set; } = 15.0f; // New property for spread angle

    public int NumProjectiles { get; set; } = 1;
    public float ShootDelay { get; set; }

    private static readonly PackedScene BulletPrefab = GD.Load<PackedScene>("res://objects/laser/laser.tscn");
    private readonly RandomNumberGenerator _rng = new();

    public void Shoot()
    {
        for (var i = 0; i < NumProjectiles; i++)
        {
            SpawnProjectile();
        }
    }

    private void SpawnProjectile()
    {
        Node parent = GetParent();
        BulletObject bulletObject = BulletPrefab.Instantiate<BulletObject>();
        bulletObject.Damage = ProjectileDamage;
        bulletObject.Color = ProjectileColor;
        bulletObject.AddCollisionExceptionWith(this);
        bulletObject.AddCollisionExceptionWith(parent);
        
        float halfSpread = Mathf.DegToRad(ProjectileSpread * 0.5f);
        float randomAngleX = _rng.RandfRange(-halfSpread * 0.1f, halfSpread * 0.1f);
        float randomAngleY = _rng.RandfRange(-halfSpread, halfSpread);

        Transform3D bulletTransform = GlobalTransform * ProjectileOrigin;
        bulletTransform = bulletTransform.RotatedLocal(Vector3.Right, randomAngleX); // X-axis rotation
        bulletTransform = bulletTransform.RotatedLocal(Vector3.Up, randomAngleY);    // Y-axis rotation

        GetTree().Root.AddChild(bulletObject);
        bulletObject.GlobalTransform = bulletTransform;
    }
}