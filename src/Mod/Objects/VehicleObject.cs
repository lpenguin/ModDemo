using Godot;

namespace ModDemo.Mod.Objects;

public partial class VehicleObject : VehicleBody3D
{
    [Export]
    public float BrakeForce { get; set; }
    [Export]
    public float MaxSteeringAngle { get; set; }
}
    [Export] public bool ControlledByPlayer { get; set; }