using Godot;

namespace ModDemo.Mod.Objects;

public enum GearState
{
    Forward,
    Reverse
}

public partial class VehicleObject : VehicleBody3D
{
    [Export] public float MaxBrakeForce { get; set; }
    [Export] public float MaxSteeringAngle { get; set; }
    [Export] public float MaxEngineForce { get; set; } = 100.0f;
    [Export] public float SteeringSpeed { get; set; } = 5.0f;
    [Export] public bool ControlledByPlayer { get; set; }
    
    private const float VELOCITY_THRESHOLD = 0.5f;
    private GearState currentGear = GearState.Forward;
    private float currentSteering = 0.0f;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (!ControlledByPlayer) return;
        
        bool isNearlyStopped = LinearVelocity.Length() < VELOCITY_THRESHOLD;

        // Handle gear changes when nearly stopped
        if (isNearlyStopped)
        {
            if (currentGear == GearState.Forward && Input.IsActionPressed("move_backward"))
            {
                currentGear = GearState.Reverse;
            }
            else if (currentGear == GearState.Reverse && Input.IsActionPressed("move_forward"))
            {
                currentGear = GearState.Forward;
            }
        }

        // Handle acceleration and braking based on current gear
        if (currentGear == GearState.Forward)
        {
            if (Input.IsActionPressed("move_forward"))
            {
                EngineForce = MaxEngineForce;
                Brake = 0;
            }
            else if (Input.IsActionPressed("move_backward"))
            {
                EngineForce = 0;
                Brake = MaxBrakeForce;
            }
            else
            {
                EngineForce = 0;
                Brake = 0;
            }
        }
        else // Reverse gear
        {
            if (Input.IsActionPressed("move_forward"))
            {
                EngineForce = 0;
                Brake = MaxBrakeForce;
            }
            else if (Input.IsActionPressed("move_backward"))
            {
                EngineForce = -MaxEngineForce;
                Brake = 0;
            }
            else
            {
                EngineForce = 0;
                Brake = 0;
            }
        }

        // Handle steering
        float targetSteering = Input.GetAxis("steer_right", "steer_left") * MaxSteeringAngle;
        // Smooth steering transition
        currentSteering = Mathf.MoveToward(currentSteering, targetSteering, SteeringSpeed * (float)delta);
        Steering = currentSteering;
    }
}
