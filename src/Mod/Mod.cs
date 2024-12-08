using System.Collections.Generic;
using System.Linq;
using Godot;
using ModDemo.Json.Levels;
using ModDemo.Json.Objects;
using ModDemo.Util;

namespace ModDemo.Mod;

public class Mod
{
    private const string LevelsDirectory = "levels";
    private const string ObjectsDirectory = "objects";
    private const string ObjectsFile = "objects.json";

    private readonly string _modDirectory;
    private readonly Dictionary<string, ObjectDefinition> _objectDefinitions;
    private readonly List<string> _levelsNames;

    public IReadOnlyDictionary<string, ObjectDefinition> ObjectDefinitions => _objectDefinitions;
    public IReadOnlyList<string> LevelsNames => _levelsNames;

    public Mod(string modDirectory)
    {
        _modDirectory = modDirectory;
        _objectDefinitions = ObjectsReader.LoadFromFile(GodotPath.Combine(modDirectory, ObjectsFile))
            .Objects
            .ToDictionary(o => o.Id, o => o);
        _levelsNames = GodotPath.GetFilesInDirectory(GodotPath.Combine(modDirectory, LevelsDirectory))
            .Where(name => name.EndsWith(".json"))
            .Select(name => name.Replace(".json", ""))
            .ToList();
    }

    public Level LoadLevel(string levelName)
    {
        return LevelReader.LoadFromFile(GodotPath.Combine(_modDirectory, LevelsDirectory, levelName + ".json"));
    }

    public Resource LoadResource(string path)
    {
        return ResourceLoader.Load(GetResourcePath(path));
    }

    public string GetResourcePath(string path)
    {
        return GodotPath.Combine(_modDirectory, ObjectsDirectory, path);
    }

    public T LoadResource<T>(string path) where T : Resource
    {
        return ResourceLoader.Load<T>(GetResourcePath(path));
    }
}