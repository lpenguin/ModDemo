using Godot;
using ModDemo.Json.Objects;
using ModDemo.Json.Common.Extensions;

namespace ModDemo.Editor;

public partial class LevelEditorObject : Node3D
{
    public string ObjectId { get; }

    public LevelEditorObject(string objectId)
    {
        ObjectId = objectId;
    }
}