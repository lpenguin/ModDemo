using Godot;
using Godot.Collections;
using ModDemo.Core;

namespace ModDemo.Game.Objects;

public enum GearState
{
    Forward,
    Reverse
}

public partial class VehicleObject : VehicleBody3D
{
    public const string SignalNameVehiclePossessed = "VehiclePossessed"; 

    [Export] public float MaxBrakeForce { get; set; }
    [Export] public float MaxSteeringAngle { get; set; }
    [Export] public float MaxEngineForce { get; set; } = 100.0f;
    [Export] public float SteeringSpeed { get; set; } = 5.0f;
    [Export] public float AccelerationRate { get; set; } = 200.0f; // Force increase per second
    [Export] public float DecelerationRate { get; set; } = 150.0f; // Force decrease per second
    [Export] public float MaxSpeed { get; set; } = 30.0f; // Maximum speed in meters per second
    [Export] public bool ControlledByPlayer { get; set; }
    [Export] public Vector3[] WeaponSlots { get; set; }

    private const float VELOCITY_THRESHOLD = 0.5f;
    private GearState currentGear = GearState.Forward;
    private float currentSteering = 0.0f;
    private float currentEngineForce = 0.0f;
    private Dictionary<int, WeaponObject> _weapons = new();
    public override void _Ready()
    {
        AddToGroup("vehicles");
        if (ControlledByPlayer)
        {
            SignalBus.DispatchSignal(SignalNameVehiclePossessed, this);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        GD.Print(LinearVelocity.Length());
         if (!ControlledByPlayer) return;
        
        bool isNearlyStopped = LinearVelocity.Length() < VELOCITY_THRESHOLD;

        // Handle gear changes when nearly stopped
        if (isNearlyStopped)
        {
            if (currentGear == GearState.Forward && Input.IsActionPressed("move_backward"))
            {
                currentGear = GearState.Reverse;
                currentEngineForce = 0; // Reset force when changing gears
            }
            else if (currentGear == GearState.Reverse && Input.IsActionPressed("move_forward"))
            {
                currentGear = GearState.Forward;
                currentEngineForce = 0; // Reset force when changing gears
            }
        }

        float currentSpeed = LinearVelocity.Length();
        float speedRatio = currentSpeed / MaxSpeed;
        float effectiveMaxForce = MaxEngineForce * (1.0f - Mathf.Clamp(speedRatio, 0.0f, 1.0f));

        // Handle acceleration and braking based on current gear
        if (currentGear == GearState.Forward)
        {
            if (Input.IsActionPressed("move_forward"))
            {
                currentEngineForce = Mathf.MoveToward(currentEngineForce, effectiveMaxForce, 
                    AccelerationRate * (float)delta);
                EngineForce = currentEngineForce;
                Brake = 0;
            }
            else if (Input.IsActionPressed("move_backward"))
            {
                currentEngineForce = Mathf.MoveToward(currentEngineForce, 0, 
                    DecelerationRate * (float)delta);
                EngineForce = currentEngineForce;
                Brake = MaxBrakeForce;
            }
            else
            {
                currentEngineForce = Mathf.MoveToward(currentEngineForce, 0, 
                    DecelerationRate * (float)delta);
                EngineForce = currentEngineForce;
                Brake = 0;
            }
        }
        else // Reverse gear
        {
            if (Input.IsActionPressed("move_forward"))
            {
                currentEngineForce = Mathf.MoveToward(currentEngineForce, 0, 
                    DecelerationRate * (float)delta);
                EngineForce = currentEngineForce;
                Brake = MaxBrakeForce;
            }
            else if (Input.IsActionPressed("move_backward"))
            {
                currentEngineForce = Mathf.MoveToward(currentEngineForce, -effectiveMaxForce, 
                    AccelerationRate * (float)delta);
                EngineForce = currentEngineForce;
                Brake = 0;
            }
            else
            {
                currentEngineForce = Mathf.MoveToward(currentEngineForce, 0, 
                    DecelerationRate * (float)delta);
                EngineForce = currentEngineForce;
                Brake = 0;
            }
        }
        // Handle steering
        float targetSteering = Input.GetAxis("steer_right", "steer_left") * MaxSteeringAngle;
        // Smooth steering transition
        currentSteering = Mathf.MoveToward(currentSteering, targetSteering, SteeringSpeed * (float)delta);
        Steering = currentSteering;

        if (Input.IsActionJustPressed("shoot"))
        {
            foreach (var weaponObject in _weapons.Values)
            {
                weaponObject.Shoot();
            }
        }
    }

    public void SetWeapon(int slot, WeaponObject weapon)
    {
        if (_weapons.TryGetValue(slot, out WeaponObject? oldWeapon))
        {
            RemoveChild(oldWeapon);
            oldWeapon.QueueFree();
        }
        AddChild(weapon);
        weapon.Position = WeaponSlots[slot];
        _weapons[slot] = weapon;
    }
}
