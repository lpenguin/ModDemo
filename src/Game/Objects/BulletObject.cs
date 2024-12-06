using Godot;
using ModDemo.Nodes;
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
            EffectManager.Instance.PlayEffect(EffectManager.EffectType.BulletHit, collision.GetPosition());

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