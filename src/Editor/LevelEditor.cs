using Godot;
using System.Collections.Generic;
using ModDemo.Json.Levels;
using ModDemo.Json.Common;
using ModDemo.Json.Common.Extensions;
using ModDemo.Mod;
using Vector3 = Godot.Vector3;

namespace ModDemo.Editor;

public partial class LevelEditor : Control
{
    [Export(PropertyHint.Dir)]
    public string modDirectory { get; set; } = "res://rootMod";
    
    private Mod.Mod _mod;
    private ItemList _objectsList;
    private Node3D _levelRoot;
    private Camera3D _editorCamera;
    private SubViewport _viewport;
    private LevelEditorObject? _selectedObject;
    private string? _selectedObjectId;
    private Level? _currentLevel;
    private ObjectsLoader _objectsLoader;
    private ObjectsController _objectsController;
    private FileDialog _saveFileDialog;
    private FileDialog _loadFileDialog;

    public override void _Ready()
    {
        // Get mod directory from export variable
        _mod = new Mod.Mod(modDirectory);
        _objectsLoader = new ObjectsLoader(_mod);

        // Initialize save file dialog
        _saveFileDialog = new FileDialog
        {
            Access = FileDialog.AccessEnum.Resources,
            FileMode = FileDialog.FileModeEnum.SaveFile,
            Filters = new[] { "*.json" },
            Title = "Save Level",
            RootSubfolder = _mod.LevelsDirectory,
            CurrentDir = _mod.LevelsDirectory
        };
        _saveFileDialog.FileSelected += OnSaveFileSelected;
        AddChild(_saveFileDialog);

        _loadFileDialog = new FileDialog
        {
            Access = FileDialog.AccessEnum.Resources,
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Filters = new[] { "*.json" },
            Title = "Load Level",
            RootSubfolder = _mod.LevelsDirectory,
            CurrentDir = _mod.LevelsDirectory
        };
        _loadFileDialog.FileSelected += OnLoadFileSelected;
        AddChild(_loadFileDialog);

        // Connect load button
        GetNode<Button>("%LoadLevel").Pressed += () => _loadFileDialog.Show();

        // Initialize UI elements
        _objectsList = GetNode<ItemList>("%ObjectsList");
        _viewport = GetNode<SubViewport>("%EditorViewport");
        
        // Setup 3D scene
        _levelRoot = new Node3D { Name = "LevelRoot" };
        _viewport.AddChild(_levelRoot);
        _viewport.GuiEmbedSubwindows = true;
        _viewport.PhysicsObjectPicking = true;
        
        // Ensure SubViewportContainer stretches viewport properly
        var viewportContainer = GetNode<SubViewportContainer>("%ViewportContainer");
        viewportContainer.StretchShrink = 1;
        
        // Add editor camera with controls
        _editorCamera = GetNode<Camera3D>("%EditorCamera");
        
        // Setup ObjectsController
        _objectsController = GetNode<ObjectsController>("%ObjectsController");
        _objectsController.ObjectSelected += OnObjectSelected;
        
        // Initialize objects browser
        InitializeObjectsBrowser();
            
        // Connect UI signals
        GetNode<Button>("%NewLevel").Pressed += OnNewLevel;
        GetNode<Button>("%SaveLevel").Pressed += OnSaveLevel;
    }

    private void OnLoadFileSelected(string path)
    {
        LoadLevel(path);
    }

    private void OnSaveFileSelected(string path)
    {
        try
        {
            // Extract level name from file path
            _currentLevel.Id = System.IO.Path.GetFileNameWithoutExtension(path);
            
            // Save the level using mod's save functionality
            _mod.SaveLevel(_currentLevel, path);
            
            // Show success message
            GD.Print($"Level saved successfully to: {path}");
        }
        catch (System.Exception e)
        {
            GD.PrintErr($"Failed to save level: {e.Message}");
        }
    }

    private void InitializeObjectsBrowser()
    {
        foreach (var (id, obj) in _mod.ObjectDefinitions)
        {
            _objectsList.AddItem(id);
        }
        
        // Connect signals
        _objectsList.ItemSelected += OnObjectBrowserSelected;
    }

    private void LoadLevel(string path)
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
            _currentLevel = _mod.LoadLevel(path);
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
            }
        }
    }

    private void OnObjectBrowserSelected(long index)
    {
        _selectedObjectId = _objectsList.GetItemText((int)index);
    }

    private void OnObjectSelected(LevelEditorObject? obj)
    {
        _selectedObject = obj;
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

        _saveFileDialog.Show();
    }
    
    private void DuplicateSelectedObject()
    {
        if (_selectedObject == null || _currentLevel == null) return;

        if (_mod.ObjectDefinitions.TryGetValue(_selectedObject.ObjectId, out var definition))
        {
            var duplicatedObject = _objectsLoader.LoadLevelEditorObject(definition);
        
            var offset = new Vector3(1, 0, 1);
            duplicatedObject.Transform = _selectedObject.Transform;
            duplicatedObject.Position += offset;
        
            _levelRoot.AddChild(duplicatedObject);
        
            _objectsController.SetSelectedObject(duplicatedObject);
        }
    }
    
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent is { Pressed: true, CtrlPressed: true, Keycode: Key.D })
        {
            DuplicateSelectedObject();
            GetViewport().SetInputAsHandled();
        }
    }
}