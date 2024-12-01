using Godot;

namespace ModDemo.Nodes;

public partial class CameraControls : Camera3D
{
    [Export]
    private float MovementSpeed = 5.0f;
    [Export]
    private float RotationSpeed = 0.005f;
    [Export]
    private float ZoomSpeed = 0.5f;
    [Export]
    private float MinZoom = 2.0f;
    [Export]
    private float MaxZoom = 20.0f;
    
    private Vector3 _targetPosition;
    private bool _isDragging = false;

    public override void _Ready()
    {
        _targetPosition = Position;
    }

    public override void _Process(double delta)
    {
        // WASD Movement
        Vector3 inputDir = Vector3.Zero;
        if (Input.IsActionPressed("ui_up")) inputDir.Z -= 1;
        if (Input.IsActionPressed("ui_down")) inputDir.Z += 1;
        if (Input.IsActionPressed("ui_left")) inputDir.X -= 1;
        if (Input.IsActionPressed("ui_right")) inputDir.X += 1;

        inputDir = inputDir.Normalized();
        _targetPosition += Transform.Basis * inputDir * MovementSpeed * (float)delta;
        Position = Position.Lerp(_targetPosition, 0.5f);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Orbital Rotation with Left Mouse Button
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                _isDragging = mouseButton.Pressed;
            }
            // Zoom with Mouse Wheel
            else if (mouseButton.ButtonIndex == MouseButton.WheelUp)
            {
                _targetPosition -= Transform.Basis.Z * ZoomSpeed;
                float distance = _targetPosition.Length();
                if (distance < MinZoom)
                    _targetPosition = _targetPosition.Normalized() * MinZoom;
            }
            else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
            {
                _targetPosition += Transform.Basis.Z * ZoomSpeed;
                float distance = _targetPosition.Length();
                if (distance > MaxZoom)
                    _targetPosition = _targetPosition.Normalized() * MaxZoom;
            }
        }
        // Handle Mouse Motion for Rotation
        else if (@event is InputEventMouseMotion mouseMotion && _isDragging)
        {
            Vector2 rotation = mouseMotion.Relative;
            RotateY(-rotation.X * RotationSpeed);
            RotateObjectLocal(Vector3.Right, -rotation.Y * RotationSpeed);
        }
    }
}
