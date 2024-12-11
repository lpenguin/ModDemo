using System.Collections.Generic;
using System.Linq;
using Godot;
using ModDemo.Json.Levels;
using ModDemo.Json.Objects;
using ModDemo.Util;

namespace ModDemo.Mod;

public class Mod
{
    private const string LevelsDirectoryName = "levels";
    private const string ObjectsDirectoryName = "objects";
    private const string ObjectsFileName = "objects.json";

    private readonly string _modDirectory;
    private readonly Dictionary<string, ObjectDefinition> _objectDefinitions;
    private readonly List<string> _levelsNames;

    public IReadOnlyDictionary<string, ObjectDefinition> ObjectDefinitions => _objectDefinitions;
    public IReadOnlyList<string> LevelsNames => _levelsNames;
    public string ModDirectory => _modDirectory;
    public string LevelsDirectory => GodotPath.Combine(_modDirectory, LevelsDirectoryName);

    public Mod(string modDirectory)
    {
        _modDirectory = modDirectory;
        _objectDefinitions = ObjectsReader.LoadFromFile(GodotPath.Combine(modDirectory, ObjectsFileName))
            .Objects
            .ToDictionary(o => o.Id, o => o);
        _levelsNames = GodotPath.GetFilesInDirectory(LevelsDirectory)
            .Where(name => name.EndsWith(".json"))
            .Select(name => name.Replace(".json", ""))
            .ToList();
    }

    public Level LoadLevel(string path)
    {
        return LevelReader.LoadFromFile(path);
    }

    public Resource LoadResource(string path)
    {
        return ResourceLoader.Load(GetResourcePath(path));
    }

    public string GetResourcePath(string path)
    {
        return GodotPath.Combine(_modDirectory, ObjectsDirectoryName, path);
    }

    public T LoadResource<T>(string path) where T : Resource
    {
        return ResourceLoader.Load<T>(GetResourcePath(path));
    }

    public void SaveLevel(Level currentLevel, string fileName)
    {
        LevelReader.SaveToFile(currentLevel, fileName);
    }
}