using Godot;
using ModDemo.Core;
using ModDemo.Game.Objects;

namespace ModDemo.Nodes;

public partial class VehicleCamera : Camera3D
{
    [Export] public float FollowDistance = 5.0f;
    [Export] public float MinDistance = 2.0f;
    [Export] public float MaxDistance = 10.0f;
    [Export] public float ZoomSpeed = 0.5f;
    [Export] public float HeightOffset = 2.0f;
    [Export] public float PositionSmoothSpeed = 5.0f;
    [Export] public float ReturnToDefaultDelay = 1.5f;
    [Export] public float ReturnSpeed = 2.0f;
    [Export] public float MouseSensitivity = 0.003f;
    [Export] public float MaxVerticalOffset = 3.0f;
    [Export] public float MaxHorizontalOffset = 10.0f;
    
    private VehicleObject? _targetVehicle;
    private Vector3 _targetPosition;
    private float _currentDistance;
    private float _lastInputTime;
    private Vector2 _cameraOffset = Vector2.Zero; // X = horizontal, Y = vertical offset
    
    public override void _Ready()
    {
        SignalBus.Instance.SignalDispatched += (name, args) =>
        {
            if (name == VehicleObject.SignalNameVehiclePossessed && args.As<VehicleObject>() is { } vehicle)
            {
                Setup(vehicle);
            }
        };
    }

    private void Setup(VehicleObject vehicle)
    {
        _targetVehicle = vehicle;
        _currentDistance = FollowDistance;
        _cameraOffset = Vector2.Zero;
        
        if (_targetVehicle == null)
        {
            GD.PrintErr("VehicleCamera: No player-controlled vehicle found!");
            return;
        }
        
        UpdateCameraPosition();
    }

    public override void _Process(double delta)
    {
        if (_targetVehicle == null) return;

        // Check if we should return to default position
        float timeSinceInput = (float)Time.GetTicksMsec() / 1000.0f - _lastInputTime;
        if (timeSinceInput > ReturnToDefaultDelay)
        {
            // Smoothly return to default position
            _cameraOffset = _cameraOffset.Lerp(Vector2.Zero, ReturnSpeed * (float)delta);
        }
        
        UpdateCameraPosition();
        
        // Smooth camera position
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
            _lastInputTime = (float)Time.GetTicksMsec() / 1000.0f;
            
            if (mouseButton.ButtonIndex == MouseButton.WheelUp)
            {
                _currentDistance = Mathf.Max(_currentDistance - ZoomSpeed, MinDistance);
            }
            else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
            {
                _currentDistance = Mathf.Min(_currentDistance + ZoomSpeed, MaxDistance);
            }
        }
        else if (inputEvent is InputEventMouseMotion mouseMotion)
        {
            _lastInputTime = (float)Time.GetTicksMsec() / 1000.0f;
            
            // Update camera offset based on mouse movement
            _cameraOffset += new Vector2(
                mouseMotion.Relative.X * MouseSensitivity,
                mouseMotion.Relative.Y * MouseSensitivity
            );
            
            // Clamp the offset
            _cameraOffset = new Vector2(
                Mathf.Clamp(_cameraOffset.X, -MaxHorizontalOffset, MaxHorizontalOffset),
                Mathf.Clamp(_cameraOffset.Y, -MaxVerticalOffset, MaxVerticalOffset)
            );
        }
    }

    private void UpdateCameraPosition()
    {
        Vector3 vehiclePos = _targetVehicle.GlobalPosition;
        
        // Get vehicle's backward direction (we want camera to be behind)
        Vector3 vehicleBack = -_targetVehicle.Transform.Basis.Z;
        Vector3 vehicleRight = _targetVehicle.Transform.Basis.X;
        
        // Apply offsets relative to vehicle orientation
        Vector3 baseOffset = vehicleBack * _currentDistance;
        Vector3 horizontalOffset = vehicleRight * _cameraOffset.X;
        Vector3 verticalOffset = Vector3.Up * (_cameraOffset.Y + HeightOffset + 2);
        
        // Calculate final camera position
        _targetPosition = vehiclePos + baseOffset + horizontalOffset + verticalOffset;
        
        // Prevent camera from clipping through walls
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