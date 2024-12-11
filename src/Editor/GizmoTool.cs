using Godot;

namespace ModDemo.Editor;

public partial class GizmoTool : Node3D
{
    [Signal]
    public delegate void ObjectTransformedEventHandler(LevelEditorObject selectedObject);

    // Colors for axes and planes
    private static readonly Color XAxisColor = new(1, 0, 0); // Red
    private static readonly Color YAxisColor = new(0, 1, 0); // Green
    private static readonly Color ZAxisColor = new(0, 0, 1); // Blue
    
    private static readonly Color XYPlaneColor = new(0, 0, 1, 0.2f); // Blue
    private static readonly Color XZPlaneColor = new(0, 1, 0, 0.2f); // Green
    private static readonly Color YZPlaneColor = new(1, 0, 0, 0.2f); // Red
    
    // Size constants
    public const float BaseAxisLength = 1.0f;
    public const float BasePlaneSize = 0.8f;
    
    // Scale settings
    private const float BaseScale = 0.1f;     // Base scale factor
    private const float MinScale = 0.5f;      // Minimum scale limit
    private const float MaxScale = 5.0f;      // Maximum scale limit
    
    // Transform state
    private Node3D _selectedObject;
    private bool _isDragging;
    private Vector3 _dragStart;
    private Vector3 _objectStartPosition;
    private Camera3D _camera;
    
    // Gizmo parts
    private MeshInstance3D _xAxis;
    private MeshInstance3D _yAxis;
    private MeshInstance3D _zAxis;
    private MeshInstance3D _xyPlane;
    private MeshInstance3D _xzPlane;
    private MeshInstance3D _yzPlane;
    
    // Ray testing
    private readonly GizmoRayTester _rayTester = new();
    
    // Movement state
    public enum DragMode
    {
        None,
        XAxis,
        YAxis,
        ZAxis,
        XYPlane,
        XZPlane,
        YZPlane
    }
    
    private DragMode _currentDragMode = DragMode.None;
    
    public override void _Ready()
    {
        CreateGizmoGeometry();
        Visible = false; // Hide by default
    }

    public override void _Process(double delta)
    {
        if (_camera != null)
        {
            // Update scale based on camera distance
            float scale = CalculateScale();
            Scale = new Vector3(scale, scale, scale);
        }
    }
    
    private float CalculateScale()
    {
        if (_camera == null) return 1.0f;
        
        // Calculate distance from camera to gizmo
        float distance = GlobalPosition.DistanceTo(_camera.GlobalPosition);
        
        // Calculate scale based on distance
        float scale = distance * BaseScale;
        
        // Clamp scale to reasonable limits
        return Mathf.Clamp(scale, MinScale, MaxScale);
    }
    
    private void CreateGizmoGeometry()
    {
        // Create axes
        _xAxis = CreateAxisMesh(Vector3.Right, XAxisColor);
        _yAxis = CreateAxisMesh(Vector3.Up, YAxisColor);
        _zAxis = CreateAxisMesh(Vector3.Forward, ZAxisColor);
        
        // Create planes - corrected orientations
        // XY plane (blue) - faces Z axis
        _xyPlane = CreatePlaneMesh(Vector3.Right, Vector3.Up, XYPlaneColor);
        
        // XZ plane (green) - faces Y axis
        _xzPlane = CreatePlaneMesh(Vector3.Right, Vector3.Back, XZPlaneColor);
        
        // YZ plane (red) - faces X axis
        _yzPlane = CreatePlaneMesh(Vector3.Up, Vector3.Back, YZPlaneColor);
        AddChild(_xAxis);
        AddChild(_yAxis);
        AddChild(_zAxis);
        AddChild(_xyPlane);
        AddChild(_xzPlane);
        AddChild(_yzPlane);
    }
    
    private MeshInstance3D CreateAxisMesh(Vector3 direction, Color color)
    {
        var mesh = new ArrayMesh();
        var vertices = new Vector3[]
        {
            Vector3.Zero,
            direction * BaseAxisLength
        };
        
        // var colors = new Color[]
        // {
        //     color,
        //     color
        // };
        
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = vertices;
        // arrays[(int)Mesh.ArrayType.Color] = colors;
        
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Lines, arrays);
        
        var material = new StandardMaterial3D
        {
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            VertexColorUseAsAlbedo = false,
            AlbedoColor = color,
            RenderPriority = 100, // Higher priority to render on top
            NoDepthTest = true    // Ignore depth buffer
        };

        return new MeshInstance3D
        {
            Mesh = mesh,
            MaterialOverride = material,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
    }
    
    private MeshInstance3D CreatePlaneMesh(Vector3 right, Vector3 up, Color color)
    {
        var planeMesh = new PlaneMesh
        {
            Size = new Vector2(BasePlaneSize, BasePlaneSize),
            Orientation = PlaneMesh.OrientationEnum.Z
        };
        
        var material = new StandardMaterial3D
        {
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            AlbedoColor = color,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            RenderPriority = 100,
            NoDepthTest = true,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled
        };

        var instance = new MeshInstance3D
        {
            Mesh = planeMesh,
            MaterialOverride = material,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        
        // Calculate the forward vector based on right and up vectors
        var forward = right.Cross(up);
        
        // Create the basis for the plane's orientation
        var basis = new Basis(right, up, forward);
        instance.Transform = new Transform3D(basis, Vector3.Zero);
        
        return instance;
    }
    
    public void SetCamera(Camera3D camera)
    {
        _camera = camera;
    }
    
    public void SetSelectedObject(Node3D obj)
    {
        _selectedObject = obj;
        if (_selectedObject != null)
        {
            GlobalPosition = _selectedObject.GlobalPosition;
            Visible = true;
        }
        else
        {
            Visible = false;
        }
    }
    
    public DragMode GetDragModeFromMousePosition(Vector2 mousePos)
    {
        if (_camera == null) return DragMode.None;
        
        var ray = GizmoRayTester.CreateRayFromCamera(_camera, mousePos);
        var hit = _rayTester.TestGizmo(ray, GlobalTransform);
        
        return hit.Type switch
        {
            GizmoRayTester.RayHit.HitType.Axis => hit.ElementIndex switch
            {
                0 => DragMode.XAxis,
                1 => DragMode.YAxis,
                2 => DragMode.ZAxis,
                _ => DragMode.None
            },
            GizmoRayTester.RayHit.HitType.Plane => hit.ElementIndex switch
            {
                0 => DragMode.XYPlane,
                1 => DragMode.XZPlane,
                2 => DragMode.YZPlane,
                _ => DragMode.None
            },
            _ => DragMode.None
        };
    }
    
    private Vector3 GetDragPlaneNormal(DragMode mode)
    {
        // Get camera direction
        var cameraDir = (_camera.GlobalPosition - GlobalPosition).Normalized();
        
        return mode switch
        {
            DragMode.XAxis => cameraDir.Cross(Vector3.Right).Cross(Vector3.Right).Normalized(),
            DragMode.YAxis => cameraDir.Cross(Vector3.Up).Cross(Vector3.Up).Normalized(),
            DragMode.ZAxis => cameraDir.Cross(Vector3.Forward).Cross(Vector3.Forward).Normalized(),
            DragMode.XYPlane => Vector3.Forward,
            DragMode.XZPlane => Vector3.Up,
            DragMode.YZPlane => Vector3.Right,
            _ => Vector3.Up
        };
    }
    
    private Vector3 GetMousePositionOnDragPlane(Vector2 mousePos)
    {
        if (_camera == null) return Vector3.Zero;
        
        var ray = GizmoRayTester.CreateRayFromCamera(_camera, mousePos);
        var planeNormal = GetDragPlaneNormal(_currentDragMode);
        var denominator = planeNormal.Dot(ray.Direction);
        
        if (Mathf.Abs(denominator) < 1e-6f)
            return GlobalPosition;
            
        var t = planeNormal.Dot(GlobalPosition - ray.Origin) / denominator;
        return ray.Origin + ray.Direction * t;
    }
    
    public void StartDrag(Vector2 mousePos, DragMode mode)
    {
        if (_selectedObject == null) return;
        
        _isDragging = true;
        _currentDragMode = mode;
        _dragStart = GetMousePositionOnDragPlane(mousePos);
        _objectStartPosition = _selectedObject.GlobalPosition;
    }
    
    public void UpdateDrag(Vector2 mousePos)
    {
        if (!_isDragging || _selectedObject == null) return;
        
        var currentPosition = GetMousePositionOnDragPlane(mousePos);
        var delta = currentPosition - _dragStart;
        
        switch (_currentDragMode)
        {
            case DragMode.XAxis:
                delta = new Vector3(delta.X, 0, 0);
                break;
            case DragMode.YAxis:
                delta = new Vector3(0, delta.Y, 0);
                break;
            case DragMode.ZAxis:
                delta = new Vector3(0, 0, delta.Z);
                break;
            case DragMode.XYPlane:
                delta = new Vector3(delta.X, delta.Y, 0);
                break;
            case DragMode.XZPlane:
                delta = new Vector3(delta.X, 0, delta.Z);
                break;
            case DragMode.YZPlane:
                delta = new Vector3(0, delta.Y, delta.Z);
                break;
        }
        
        _selectedObject.GlobalPosition = _objectStartPosition + delta;
        EmitSignal(SignalName.ObjectTransformed, _selectedObject);
        GlobalPosition = _selectedObject.GlobalPosition;
    }
    
    public void EndDrag()
    {
        _isDragging = false;
        _currentDragMode = DragMode.None;
    }
    
    public void HighlightPart(DragMode mode)
    {
        // Reset all highlights
        SetHighlight(_xAxis, XAxisColor);
        SetHighlight(_yAxis, YAxisColor);
        SetHighlight(_zAxis, ZAxisColor);
        SetHighlight(_xyPlane, XYPlaneColor);
        SetHighlight(_xzPlane, XZPlaneColor);
        SetHighlight(_yzPlane, YZPlaneColor);
        
        // Set highlight based on mode
        switch (mode)
        {
            case DragMode.XAxis:
                SetHighlight(_xAxis, Colors.White);
                break;
            case DragMode.YAxis:
                SetHighlight(_yAxis, Colors.White);
                break;
            case DragMode.ZAxis:
                SetHighlight(_zAxis, Colors.White);
                break;
            case DragMode.XYPlane:
                SetHighlight(_xyPlane, Colors.White);
                break;
            case DragMode.XZPlane:
                SetHighlight(_xzPlane, Colors.White);
                break;
            case DragMode.YZPlane:
                SetHighlight(_yzPlane, Colors.White);
                break;
        }
    }

    private void SetHighlight(MeshInstance3D part, Color highlight)
    {
        if (part.MaterialOverride is StandardMaterial3D material)
        {
            material.AlbedoColor = highlight;
        }
    }
}