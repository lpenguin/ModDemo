using System;
using Godot;

namespace ModDemo.Nodes;

[GlobalClass]
public partial class EffectManager : Node
{
    private static EffectManager? _instance;
    public static EffectManager Instance => _instance ?? throw new InvalidOperationException("EffectManager is not initialized");

    private static readonly PackedScene ExplosionPrefab = GD.Load<PackedScene>("res://objects/explosion/explosion.tscn");
    private static readonly PackedScene MushroomExplosionPrefab = GD.Load<PackedScene>("res://objects/mushroomExplosion/mushroomExplosion.tscn");

    public class EffectType
    {
        public const string MushroomExplosion = "MushroomExplosion";
        public const string Explosion = "Explosion";
        public const string BulletHit = "BulletHit";
    }

    public override void _Ready()
    {
        _instance = this;
    }

    public void PlayEffect(string effectType, Vector3 position)
    {
        switch (effectType)
        {
            case EffectType.MushroomExplosion:
                SpawnParticles(position, MushroomExplosionPrefab);
                break;
            case EffectType.Explosion:
            case EffectType.BulletHit:
                SpawnParticles(position, ExplosionPrefab);
                break;
            default:
                GD.PrintErr("Unknown effect type");
                break;
        }
    }

    private void SpawnParticles(Vector3 position, PackedScene explosionPrefab)
    {
        GpuParticles3D explosion = explosionPrefab.Instantiate<GpuParticles3D>();
        GetTree().Root.AddChild(explosion);
        explosion.GlobalPosition = position;
        explosion.Emitting = true;
    }
}