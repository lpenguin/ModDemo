using Godot;

namespace ModDemo.Nodes;

public partial class CameraControls : Camera3D
{
    [Export]
    private float MovementSpeed = 2.0f; // Reduced from 5.0f to 2.0f
    [Export]
    private float RotationSpeed = 0.005f;
    [Export]
    private float ZoomSpeed = 0.5f;
    [Export]
    private float MinZoom = 2.0f;
    [Export]
    private float MaxZoom = 20.0f;
    
    private Vector3 _focalPoint = Vector3.Zero;
    private float _distance = 10.0f;
    private float _azimuthAngle = 0.0f;   // Horizontal angle
    private float _elevationAngle = 0.5f;  // Vertical angle (in radians)
    private bool _isRotating = false;
    private bool _isPanning = false;

    public override void _Ready()
    {
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        // Clamp elevation angle to prevent camera flipping
        _elevationAngle = Mathf.Clamp(_elevationAngle, 0.1f, Mathf.Pi - 0.1f);

        // Convert spherical coordinates to Cartesian
        float x = _distance * Mathf.Sin(_elevationAngle) * Mathf.Cos(_azimuthAngle);
        float y = _distance * Mathf.Cos(_elevationAngle);
        float z = _distance * Mathf.Sin(_elevationAngle) * Mathf.Sin(_azimuthAngle);

        // Set camera position and make it look at the focal point
        Position = _focalPoint + new Vector3(x, y, z);
        LookAt(_focalPoint, Vector3.Up);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Right)
            {
                _isRotating = mouseButton.Pressed;
            }
            else if (mouseButton.ButtonIndex == MouseButton.Middle)
            {
                _isPanning = mouseButton.Pressed;
            }
            // Zoom with Mouse Wheel
            else if (mouseButton.ButtonIndex == MouseButton.WheelUp)
            {
                _distance = Mathf.Max(_distance - ZoomSpeed, MinZoom);
                UpdateCameraPosition();
            }
            else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
            {
                _distance = Mathf.Min(_distance + ZoomSpeed, MaxZoom);
                UpdateCameraPosition();
            }
        }
        // Handle Mouse Motion
        else if (@event is InputEventMouseMotion mouseMotion)
        {
            if (_isRotating)
            {
                // Orbital rotation
                _azimuthAngle -= mouseMotion.Relative.X * RotationSpeed;
                _elevationAngle += mouseMotion.Relative.Y * RotationSpeed;
                UpdateCameraPosition();
            }
            else if (_isPanning)
            {
                // Pan camera in XZ plane
                // Scale movement based on distance to make it feel more natural
                float moveScale = MovementSpeed * _distance * 0.0005f; // Reduced from 0.001f to 0.0005f
                
                // Get camera's right and forward vectors projected onto XZ plane
                Vector3 right = -Transform.Basis.X;
                right.Y = 0;
                right = right.Normalized();
                
                Vector3 forward = -Transform.Basis.Z;
                forward.Y = 0;
                forward = forward.Normalized();

                // Move focal point based on mouse motion (removed negative signs to invert axes)
                _focalPoint += right * (mouseMotion.Relative.X * moveScale);
                _focalPoint += forward * (mouseMotion.Relative.Y * moveScale);
                
                UpdateCameraPosition();
            }
        }
    }
}