using Godot;
using ModDemo.Util.Extensions;

namespace ModDemo.Game.Objects;

public partial class BulletObject: StaticBody3D
{
    [Export]
    public float Damage { get; set; }
    
    [Export]
    public Color Color { get; set; }
    
    [Export]
    public float Speed { get; set; }

    private MeshInstance3D? _meshInstance;
    
    private static readonly PackedScene ExplosionPrefab = GD.Load<PackedScene>("res://objects/explosion/explosion.tscn");
    public override void _Ready()
    {
        _meshInstance = GetNode<MeshInstance3D>("MeshInstance3D");
        // Set the color of the bullet
        StandardMaterial3D material = (StandardMaterial3D) _meshInstance.MaterialOverride;
        material.AlbedoColor = Color;
        material.Emission = Color;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Get Forward Vector
        Vector3 forward = Transform.Basis.Z;
        KinematicCollision3D? collision = MoveAndCollide(forward * Speed * (float) delta);
        if (collision != null)
        {
            GpuParticles3D explosion = ExplosionPrefab.Instantiate<GpuParticles3D>();
            GetTree().Root.AddChild(explosion);
            explosion.GlobalPosition = collision.GetPosition();
            explosion.Emitting = true;

            if (collision.GetCollider() is Node node)
            {
                if (node.TryGetChild<IDamageHandler>(out var damageHandler))
                {
                    damageHandler!.ApplyDamage(Damage);
                }
            }
            QueueFree();
        }
    }
}