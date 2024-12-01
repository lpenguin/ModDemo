namespace ModDemo.Json.Common.Extensions;

public static class Vector3Ext
{
    public static Godot.Vector3 ToGodot(this Vector3 vector)
    {
        return new Godot.Vector3(vector.X, vector.Y, vector.Z);
    }
}