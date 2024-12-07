using System.Collections.Generic;
using Godot;
using ModDemo.Game.Objects;
using ModDemo.Json.Levels;
using ModDemo.Nodes;
using ModDemo.Json.Common;
using ModDemo.Mod;
using ModDemo.Util;
using Color = System.Drawing.Color;
using Vector3 = Godot.Vector3;

namespace ModDemo.Editor;

public partial class LevelEditor : Control
{
    [Export] private NodePath modDirectoryPath;  // Path to mod directory in project settings
    
    private string _modDirectory;
    private ObjectsCollection _objectsCollection;
    private ItemList _objectsList;
    private Node3D _levelRoot;
    private Camera3D _editorCamera;
    private SubViewport _viewport;
    private Node3D? _selectedObject;
    private string? _selectedObjectId;
    private Level? _currentLevel;
    private PackedScene _levelPickerScene;
    private LevelPicker _levelPicker;

    public override void _Ready()
    {
        // Get mod directory from export variable
        _modDirectory = GodotPath.Combine("res://", "rootMod");
        
        _levelPickerScene = GD.Load<PackedScene>("res://levelEditor/level_picker.tscn");
        _levelPicker = _levelPickerScene.Instantiate<LevelPicker>();
        _levelPicker.Initialize(_modDirectory);
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
        _editorCamera = new CameraControls();
        _editorCamera.Position = new Vector3(0, 5, 10);
        _viewport.AddChild(_editorCamera);
        
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
        // Get objects collection from mod directory
        _objectsCollection = ObjectsLoader.Load(_modDirectory);
        
        // Populate objects list
        foreach (var obj in _objectsCollection.GetAllObjects())
        {
            _objectsList.AddItem(obj.Key);
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

        // Construct level file path
        var levelPath = GodotPath.Combine(_modDirectory, "levels", $"{levelName}.json");
        
        // Load level data
        try
        {
            _currentLevel = LevelReader.LoadFromFile(levelPath);
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
            if (_objectsCollection.TryGetObject(levelObject.ObjectId, out Node3D? template))
            {
                var instance = (Node3D)template.Duplicate();
                
                // Apply transform from level data
                if (levelObject.Transform != null)
                {
                    if (levelObject.Transform.Position != null)
                    {
                        instance.Position = new Vector3(
                            levelObject.Transform.Position.X,
                            levelObject.Transform.Position.Y,
                            levelObject.Transform.Position.Z
                        );
                    }
                    
                    if (levelObject.Transform.Rotation != null)
                    {
                        instance.Rotation = new Vector3(
                            levelObject.Transform.Rotation.X,
                            levelObject.Transform.Rotation.Y,
                            levelObject.Transform.Rotation.Z
                        );
                    }
                    
                    if (levelObject.Transform.Scale != null)
                    {
                        instance.Scale = new Vector3(
                            levelObject.Transform.Scale.X,
                            levelObject.Transform.Scale.Y,
                            levelObject.Transform.Scale.Z
                        );
                    }
                }

                // Store the original object ID for saving
                instance.Name = levelObject.ObjectId;
                
                // Add to scene
                _levelRoot.AddChild(instance);
                
                // Add label with object name if specified
                if (!string.IsNullOrEmpty(levelObject.Name))
                {
                    var label3D = new Label3D
                    {
                        Text = levelObject.Name,
                        Position = instance.Position + Vector3.Up * 2,
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

    private void SelectObject(Node3D obj)
    {
        if (_selectedObject != null)
        {
            // Remove highlight from previously selected object
            if (_selectedObject is MeshInstance3D prevMesh)
            {
                // Reset material or highlight
            }
        }

        _selectedObject = obj;

        if (_selectedObject != null)
        {
            // Highlight new selected object
            if (_selectedObject is MeshInstance3D newMesh)
            {
                // Add highlight material or effect
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
            {
                HandleLeftClick(mouseButton);
            }
            else if (mouseButton.ButtonIndex == MouseButton.Right && mouseButton.Pressed)
            {
                HandleRightClick(mouseButton);
            }
        }
    }

    private void HandleLeftClick(InputEventMouseButton mouseButton)
    {
        if (_selectedObjectId == null) return;

        // Cast ray from camera to find placement position
        var mousePos = GetViewport().GetMousePosition();
        var rayOrigin = _editorCamera.ProjectRayOrigin(mousePos);
        var rayEnd = rayOrigin + _editorCamera.ProjectRayNormal(mousePos) * 1000;
        
        var spaceState = _viewport.World3D.DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
        var result = spaceState.IntersectRay(query);
        
        if (result.Count > 0)
        {
            var hitPosition = (Vector3)result["position"];
            PlaceObject(_selectedObjectId, hitPosition);
        }
    }

    private void HandleRightClick(InputEventMouseButton mouseButton)
    {
        // Cast ray for object selection
        var mousePos = GetViewport().GetMousePosition();
        var rayOrigin = _editorCamera.ProjectRayOrigin(mousePos);
        var rayEnd = rayOrigin + _editorCamera.ProjectRayNormal(mousePos) * 1000;
        
        var spaceState = _viewport.World3D.DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
        var result = spaceState.IntersectRay(query);
        
        if (result.Count > 0 && result.ContainsKey("collider"))
        {
            var hitObject = (Node3D)result["collider"];
            SelectObject(hitObject);
        }
        else
        {
            SelectObject(null);
        }
    }
    
    private void PlaceObject(string objectId, Vector3 position)
    {
        if (_objectsCollection.TryGetObject(objectId, out Node3D? template))
        {
            var instance = (Node3D)template.Duplicate();
            instance.Position = position;
            _levelRoot.AddChild(instance);
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
            if (child is Node3D node3D && child != _editorCamera && !(child is MeshInstance3D) && !(child is Label3D))
            {
                var levelObject = new LevelObject
                {
                    ObjectId = node3D.Name, // You might want to store the original object ID somewhere
                    Transform = new Transform
                    {
                        Position = new Json.Common.Vector3
                        {
                            X = node3D.Position.X,
                            Y = node3D.Position.Y,
                            Z = node3D.Position.Z
                        },
                        Rotation = new Json.Common.Vector3
                        {
                            X = node3D.Rotation.X,
                            Y = node3D.Rotation.Y,
                            Z = node3D.Rotation.Z
                        },
                        Scale = new Json.Common.Vector3
                        {
                            X = node3D.Scale.X,
                            Y = node3D.Scale.Y,
                            Z = node3D.Scale.Z
                        }
                    }
                };
                _currentLevel.Objects.Add(levelObject);
            }
        }

        // Save level to file
        // You'll need to implement this part based on your save system
    }
}