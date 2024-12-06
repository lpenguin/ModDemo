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
	private string modDirectory = "res://rootMod";
	
	private ObjectsCollection _objects;
	private List<Level> _levels;

	public override void _Ready()
	{
		_objects = ObjectsLoader.Load(modDirectory);
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
		var levelsDirectory = GodotPath.Combine(modDirectory, "levels");
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
