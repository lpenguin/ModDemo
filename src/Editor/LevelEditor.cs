using Godot;
using System.Collections.Generic;
using ModDemo.Json.Objects;
using ModDemo.Json.Levels;
using ModDemo.Json.Common;
using ModDemo.Json.Common.Extensions;
using ModDemo.Mod;
using ModDemo.Nodes;
using ModDemo.Util;
using Vector3 = Godot.Vector3;

namespace ModDemo.Editor;

public partial class LevelEditor : Control
{
    [Export(PropertyHint.Dir)]
    public string modDirectory { get; set; } = "res://rootMod"; // Path to mod directory in project settings
    
    private Mod.Mod _mod;
    private ItemList _objectsList;
    private Node3D _levelRoot;
    private Camera3D _editorCamera;
    private SubViewport _viewport;
    private LevelEditorObject? _selectedObject;
    private string? _selectedObjectId;
    private Level? _currentLevel;
    private PackedScene _levelPickerScene;
    private LevelPicker _levelPicker;
    private ObjectsLoader _objectsLoader;

    public override void _Ready()
    {
        // Get mod directory from export variable
        _mod = new Mod.Mod(modDirectory);
        _objectsLoader = new ObjectsLoader(_mod);

        _levelPickerScene = GD.Load<PackedScene>("res://levelEditor/level_picker.tscn");
        _levelPicker = _levelPickerScene.Instantiate<LevelPicker>();
        _levelPicker.Initialize(_mod);
        _levelPicker.LevelSelected += OnLevelSelected;
        AddChild(_levelPicker);

        // Connect load button
        GetNode<Button>("%LoadLevel").Pressed += () => _levelPicker.Show();

        // Initialize UI elements
        _objectsList = GetNode<ItemList>("%ObjectsList");
        _viewport = GetNode<SubViewport>("%EditorViewport");
        
        // Setup 3D scene
        _levelRoot = new Node3D();
        _viewport.AddChild(_levelRoot);
        _viewport.GuiEmbedSubwindows = true;
        _viewport.PhysicsObjectPicking = true;
    
        // Ensure SubViewportContainer stretches viewport properly
        var viewportContainer = GetNode<SubViewportContainer>("%ViewportContainer");
        viewportContainer.StretchShrink = 1;
        viewportContainer.MouseFilter = MouseFilterEnum.Pass;
        
        // Add editor camera with controls
        _editorCamera = GetNode<Camera3D>("%EditorCamera");
        
        // Add grid for reference
        AddEditorGrid();
        
        // Initialize objects browser
        InitializeObjectsBrowser();
            
        // Connect UI signals
        GetNode<Button>("%NewLevel").Pressed += OnNewLevel;
        GetNode<Button>("%SaveLevel").Pressed += OnSaveLevel;
    }

    private void OnLevelSelected(string levelName)
    {
        LoadLevel(levelName);
    }

    private void InitializeObjectsBrowser()
    {

        foreach (var (id, obj) in _mod.ObjectDefinitions)
        {
            _objectsList.AddItem(id);
        }
        
        // Connect signals
        _objectsList.ItemSelected += OnObjectSelected;
    }

    private void LoadLevel(string levelName)
    {
        // Clear existing level objects
        foreach (Node child in _levelRoot.GetChildren())
        {
            if (child != _editorCamera && !(child is MeshInstance3D)) // Don't remove camera and grid
            {
                child.QueueFree();
            }
        }

        // Load level data from JSON
        try
        {
            _currentLevel = _mod.LoadLevel(levelName);
        }
        catch (System.Exception e)
        {
            GD.PrintErr($"Failed to load level: {e.Message}");
            return;
        }

        if (_currentLevel == null) return;

        // Create objects defined in the level
        foreach (var levelObject in _currentLevel.Objects)
        {
            if (_mod.ObjectDefinitions.TryGetValue(levelObject.ObjectId, out var definition))
            {
                var editorObject = _objectsLoader.LoadLevelEditorObject(definition);

                // Apply transform from level data
                if (levelObject.Transform != null)
                {
                    editorObject.Transform = levelObject.Transform.ToGodot();
                }

                _levelRoot.AddChild(editorObject);
                    
                // Add label with object name if specified
                if (!string.IsNullOrEmpty(levelObject.Name))
                {
                    var label3D = new Label3D
                    {
                        Text = levelObject.Name,
                        Position = editorObject.Position + Vector3.Up * 2,
                        Scale = new Vector3(0.5f, 0.5f, 0.5f),
                        Billboard = BaseMaterial3D.BillboardModeEnum.Enabled
                    };
                        
                    _levelRoot.AddChild(label3D);
                }
            }
        }
    }

    private void OnObjectSelected(long index)
    {
        _selectedObjectId = _objectsList.GetItemText((int)index);
    }

    private void SelectObject(LevelEditorObject obj)
    {
        _selectedObject = obj;
        // TODO: Add visual selection feedback
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
            {
                HandleLeftClick();
            }
            else if (mouseButton.ButtonIndex == MouseButton.Right && mouseButton.Pressed)
            {
                HandleRightClick(mouseButton);
            }
        }
    }

    private void HandleLeftClick()
    {
        if (_selectedObjectId == null || !_mod.ObjectDefinitions.ContainsKey(_selectedObjectId)) return;

        // Cast ray from camera to find placement position
        var mousePos = GetViewport().GetMousePosition();
        var rayOrigin = _editorCamera.ProjectRayOrigin(mousePos);
        var rayEnd = rayOrigin + _editorCamera.ProjectRayNormal(mousePos) * 1000;
        
        var spaceState = _viewport.World3D.DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
        query.CollideWithAreas = true;
        var result = spaceState.IntersectRay(query);
        
        if (result.Count > 0)
        {
            var hitPosition = (Vector3)result["position"];
            PlaceObject(_selectedObjectId, hitPosition);
        }
    }

    private void HandleRightClick(InputEventMouseButton mouseButton)
    {
        var mousePos = GetViewport().GetMousePosition();
        var rayOrigin = _editorCamera.ProjectRayOrigin(mousePos);
        var rayEnd = rayOrigin + _editorCamera.ProjectRayNormal(mousePos) * 1000;
        
        var spaceState = _viewport.World3D.DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
        query.CollideWithAreas = true;
        var result = spaceState.IntersectRay(query);
        
        if (result.Count > 0 && result.ContainsKey("collider"))
        {
            var hitObject = result["collider"].As<Node3D>();
            if (hitObject.GetParent() is LevelEditorObject editorObject)
            {
                SelectObject(editorObject);
            }
        }
        else
        {
            SelectObject(null);
        }
    }
    
    private void PlaceObject(string objectId, Vector3 position)
    {
        if (_mod.ObjectDefinitions.TryGetValue(objectId, out var definition))
        {
            var editorObject = _objectsLoader.LoadLevelEditorObject(definition);
            editorObject.Position = position;
            _levelRoot.AddChild(editorObject);
        }
    }
    
    private void AddEditorGrid()
    {
        var planeMesh = new PlaneMesh
        {
            Size = new Vector2(20, 20), // 20x20 units
            SubdivideWidth = 20,
            SubdivideDepth = 20
        };
        
        var gridMeshInstance = new MeshInstance3D
        {
            Mesh = planeMesh
        };
        
        // Create grid material
        var material = new StandardMaterial3D
        {
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            AlbedoColor = new Godot.Color(0.3f, 0.3f, 0.3f, 0.5f),
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
        };
        
        gridMeshInstance.MaterialOverride = material;
        _levelRoot.AddChild(gridMeshInstance);
    }

    private void OnNewLevel()
    {
        // Clear existing objects
        foreach (Node child in _levelRoot.GetChildren())
        {
            if (child != _editorCamera && !(child is MeshInstance3D)) // Don't remove camera and grid
            {
                child.QueueFree();
            }
        }

        _currentLevel = new Level
        {
            Id = "new_level",
            Name = "New Level",
            Objects = new List<LevelObject>()
        };
    }

    private void OnSaveLevel()
    {
        if (_currentLevel == null) return;

        // Update level data from scene
        _currentLevel.Objects.Clear();
        foreach (Node child in _levelRoot.GetChildren())
        {
            if (child is LevelEditorObject editorObject)
            {
                var levelObject = new LevelObject
                {
                    ObjectId = editorObject.ObjectId,
                    Transform = new Transform
                    {
                        Position = new Json.Common.Vector3
                        {
                            X = editorObject.Position.X,
                            Y = editorObject.Position.Y,
                            Z = editorObject.Position.Z
                        },
                        Rotation = new Json.Common.Vector3
                        {
                            X = editorObject.Rotation.X,
                            Y = editorObject.Rotation.Y,
                            Z = editorObject.Rotation.Z
                        },
                        Scale = new Json.Common.Vector3
                        {
                            X = editorObject.Scale.X,
                            Y = editorObject.Scale.Y,
                            Z = editorObject.Scale.Z
                        }
                    }
                };
                _currentLevel.Objects.Add(levelObject);
            }
        }

        // Save level to file
        // TODO: Implement save dialog and file writing
    }
}