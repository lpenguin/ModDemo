using Godot;

namespace ModDemo.Editor;

public partial class ObjectsController : Node
{
    private Camera3D _editorCamera;
    private GizmoTool _gizmoTool;
    private bool _isDraggingGizmo;
    private LevelEditorObject? _selectedObject;
    
    [Signal]
    public delegate void ObjectSelectedEventHandler(LevelEditorObject? selectedObject);

    public override void _Ready()
    {
        // Get references
        var viewport = GetParent<SubViewport>();
        _editorCamera = viewport.GetNode<Camera3D>("%EditorCamera");

        // Setup gizmo
        _gizmoTool = new GizmoTool();
        AddChild(_gizmoTool);
        _gizmoTool.SetCamera(_editorCamera);
    }

    private LevelEditorObject? GetObjectUnderMouse(Vector2 mousePos)
    {
        var rayOrigin = _editorCamera.ProjectRayOrigin(mousePos);
        var rayEnd = rayOrigin + _editorCamera.ProjectRayNormal(mousePos) * 1000;
        
        var spaceState = GetViewport().World3D.DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
        query.CollideWithAreas = true;
        var result = spaceState.IntersectRay(query);
        
        if (result.Count > 0 && result.ContainsKey("collider"))
        {
            var hitObject = result["collider"].As<Node3D>();
            if (hitObject.GetParent() is LevelEditorObject editorObject)
            {
                return editorObject;
            }
        }
        
        return null;
    }

    private void SelectObject(LevelEditorObject? obj)
    {
        _selectedObject = obj;
        _gizmoTool.SetSelectedObject(_selectedObject);
        EmitSignal(SignalName.ObjectSelected, _selectedObject);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        var mousePos = GetViewport().GetMousePosition();
        
        if (@event is InputEventMouseMotion)
        {
            if (_isDraggingGizmo)
            {
                _gizmoTool.UpdateDrag(mousePos);
            }
            else
            {
                // Update gizmo highlight
                var dragMode = _gizmoTool.GetDragModeFromMousePosition(mousePos);
                _gizmoTool.HighlightPart(dragMode);
            }
        }
        else if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                if (mouseButton.Pressed)
                {
                    // Check for gizmo interaction first
                    var dragMode = _gizmoTool.GetDragModeFromMousePosition(mousePos);
                    if (dragMode != GizmoTool.DragMode.None)
                    {
                        _isDraggingGizmo = true;
                        _gizmoTool.StartDrag(mousePos, dragMode);
                    }
                    else
                    {
                        // If not interacting with gizmo, handle object selection
                        var hitObject = GetObjectUnderMouse(mousePos);
                        SelectObject(hitObject);
                    }
                }
                else if (_isDraggingGizmo)
                {
                    // End gizmo drag
                    _isDraggingGizmo = false;
                    _gizmoTool.EndDrag();
                }
            }
        }
    }

    public void SetSelectedObject(LevelEditorObject? obj)
    {
        SelectObject(obj);
    }
}