using System.Collections.Generic;
using System.Linq;
using Godot;
using ModDemo.Game.Objects;
using ModDemo.Json.Levels;
using ModDemo.Mod;
using ModDemo.Util;

namespace ModDemo.Nodes;

[GlobalClass]
public partial class ModLoader: Node3D
{
	[Export(PropertyHint.Dir)]
	public string ModDirectory { get; set; } = "res://rootMod";
	
	private ObjectsCollection _objects;
	private List<Level> _levels;

	public override void _Ready()
	{
		var objectsLoader = new ObjectsLoader(new Mod.Mod(ModDirectory));
		_objects = objectsLoader.LoadObjects();
		_levels = LoadLevels();
	}

	public List<Level> GetLevels() => _levels.ToList();

	public Node3D InstantiateLevel(string levelName)
	{
		var level = _levels.FirstOrDefault(l => l.Name == levelName);
		if (level == null)
		{
			GD.PrintErr($"Level not found: {levelName}");
			return new Node3D();
		}

		return Mod.LevelLoader.CreateScene(level, _objects);
	}
	
	private List<Level> LoadLevels()
	{
		var levelsDirectory = GodotPath.Combine(ModDirectory, "levels");
		var levelPaths = GodotPath.GetFilesInDirectory(levelsDirectory);
		var levels = new List<Level>();

		foreach (var relativePath in levelPaths)
		{
			var fullPath = GodotPath.Combine(levelsDirectory, relativePath);
			try
			{
				var level = LevelReader.LoadFromFile(fullPath);
				if (LevelReader.Validate(level, out var errors))
				{
					levels.Add(level);
				}
				else
				{
					GD.PushError($"Failed to validate level {relativePath}: {string.Join(", ", errors)}");
				}
			}
			catch (System.Exception ex)
			{
				GD.PushError($"Failed to load level {relativePath}: {ex.Message}");
			}
		}

		return levels;
	}
}
