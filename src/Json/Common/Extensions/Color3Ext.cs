namespace ModDemo.Json.Common.Extensions;

public static class Color3Ext
{
    public static Godot.Color ToGodot(this Color color)
    {
        return new Godot.Color(color.R, color.G, color.B);
    }
}