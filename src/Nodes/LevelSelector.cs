using System.Collections.Generic;
using Godot;
using ModDemo.Json.Levels;

namespace ModDemo.Nodes;

public partial class LevelSelector : Control
{
    [Export(PropertyHint.NodeType, nameof(ModLoader))]
    private NodePath modLoaderNodePath;

    private ItemList _levelList;
    private Button _loadButton;
    private ModLoader _modLoader;
    private string _selectedLevel = "";

    public override void _Ready()
    {
        _levelList = GetNode<ItemList>("%LevelList");
        _loadButton = GetNode<Button>("%LoadButton");
        _modLoader = GetNode<ModLoader>(modLoaderNodePath);

        _modLoader.Ready += OnModLoaderReady;
        _levelList.ItemSelected += OnLevelSelected;
        _loadButton.Pressed += OnLoadButtonPressed;

        _loadButton.Disabled = true;
        if (_modLoader.IsNodeReady())
        {
            OnModLoaderReady();
        }
    }

    private void OnModLoaderReady()
    {
        List<Level> levels = _modLoader.GetLevels();
        foreach (Level level in levels)
        {
            _levelList.AddItem(level.Name);
        }
    }

    private void OnLevelSelected(long index)
    {
        _selectedLevel = _levelList.GetItemText((int)index);
        _loadButton.Disabled = false;
    }

    private void OnLoadButtonPressed()
    {
        if (!string.IsNullOrEmpty(_selectedLevel))
        {
            Node3D levelInstance = _modLoader.InstantiateLevel(_selectedLevel);
            GetTree().CurrentScene.AddChild(levelInstance);
            // Add the levelInstance to the scene tree or handle it as needed
            GD.Print($"Level loaded: {_selectedLevel}");
        }
        else
        {
            GD.Print("No level selected");
        }
    }
}
