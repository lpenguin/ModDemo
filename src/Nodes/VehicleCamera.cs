using Godot;
using ModDemo.Core;
using ModDemo.Mod.Objects;

namespace ModDemo.Nodes;

public partial class VehicleCamera : Camera3D
{
    [Export] public float FollowDistance = 5.0f;
    [Export] public float MinZoomDistance = 2.0f;
    [Export] public float MaxZoomDistance = 10.0f;
    [Export] public float ZoomSpeed = 0.5f;
    [Export] public float RotationSpeed = 2.0f;
    [Export] public float HeightOffset = 2.0f;
    [Export] public float PositionSmoothSpeed = 5.0f;
    [Export] public float RotationSmoothSpeed = 3.0f;
    [Export] public float MinVerticalAngle = -30.0f; // Changed: limited to -30 degrees to prevent seeing underneath
    [Export] public float MaxVerticalAngle = 60.0f;  // Kept at 60 degrees for looking up
    
    private VehicleObject? _targetVehicle;
    private float _currentDistance;
    private float _currentRotationAngle;
    private float _currentVerticalAngle;
    private Vector3 _targetPosition;
    
    public override void _Ready()
    {
        SignalBus.Instance.SignalDispatched += (name, args) =>
        {
            if (name == "VehiclePossessed" && args.As<VehicleObject>() is { } vehicle)
            {
                Setup(vehicle);
            }
        };
    }

    private void Setup(VehicleObject vehicle)
    {
        _currentDistance = FollowDistance;
        _currentVerticalAngle = 0.0f; // Initialize vertical angle
        
        _targetVehicle = vehicle;
        if (_targetVehicle == null)
        {
            GD.PrintErr("VehicleCamera: No player-controlled vehicle found!");
            return;
        }
        
        // Set initial position
        UpdateCameraPosition();
    }

    public override void _Process(double delta)
    {
        if (_targetVehicle == null) return;
        
        UpdateCameraPosition();
        
        // Smooth position and rotation
        Position = Position.Lerp(_targetPosition, PositionSmoothSpeed * (float)delta);
        
        // Make camera look at vehicle
        Vector3 targetLookAt = _targetVehicle.GlobalPosition + Vector3.Up * HeightOffset;
        Transform = Transform.LookingAt(targetLookAt, Vector3.Up);
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (_targetVehicle == null) return;

        if (inputEvent is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.WheelUp)
            {
                _currentDistance = Mathf.Max(_currentDistance - ZoomSpeed, MinZoomDistance);
            }
            else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
            {
                _currentDistance = Mathf.Min(_currentDistance + ZoomSpeed, MaxZoomDistance);
            }
        }
        else if (inputEvent is InputEventMouseMotion mouseMotion)
        {
            // Horizontal rotation
            _currentRotationAngle -= mouseMotion.Relative.X * RotationSpeed * 0.01f;
            
            // Vertical rotation (inverted by removing the negative sign)
            _currentVerticalAngle += mouseMotion.Relative.Y * RotationSpeed * 0.01f;
            // Clamp vertical angle to prevent over-rotation
            _currentVerticalAngle = Mathf.Clamp(_currentVerticalAngle, 
                Mathf.DegToRad(MinVerticalAngle), 
                Mathf.DegToRad(MaxVerticalAngle));
        }
    }

    private void UpdateCameraPosition()
    {
        Vector3 vehiclePos = _targetVehicle.GlobalPosition;
        
        // Calculate camera position using spherical coordinates
        float horizontalDistance = _currentDistance * Mathf.Cos(_currentVerticalAngle);
        float verticalOffset = _currentDistance * Mathf.Sin(_currentVerticalAngle);
        
        Vector3 offset = new Vector3(
            Mathf.Sin(_currentRotationAngle) * horizontalDistance,
            HeightOffset + verticalOffset,
            Mathf.Cos(_currentRotationAngle) * horizontalDistance
        );
        
        _targetPosition = vehiclePos + offset;
        
        // Ray casting to prevent camera from clipping through walls
        var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(
            vehiclePos + Vector3.Up * HeightOffset,
            _targetPosition
        );
        var result = spaceState.IntersectRay(query);
        
        if (result.Count > 0)
        {
            _targetPosition = (Vector3)result["position"];
        }
    }
}