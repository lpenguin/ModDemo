using System.Collections.Generic;
using Godot;

namespace ModDemo.Game.Objects;

public class ObjectsCollection
{
    private Dictionary<string, Node3D> _objects = new();

    public void AddObject(string id, Node3D obj)
    {
        _objects.Add(id, obj);
    }

    public Node3D GetObject(string id)
    {
        return _objects[id];
    }

    public bool TryGetObject(string id, out Node3D? obj)
    {
        return _objects.TryGetValue(id, out obj);
    }
}