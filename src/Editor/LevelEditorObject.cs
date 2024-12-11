using System.Collections.Generic;
using Godot;

namespace ModDemo.Editor;

public partial class LevelEditorObject : Node3D
{
    public string ObjectId { get; }
    public string Name { get; set; }
    
    public Dictionary<string, string> Tags { get; set; } = new();

    public LevelEditorObject(string objectId)
    {
        ObjectId = objectId;
        Name = objectId; // Default name to objectId
    }
}