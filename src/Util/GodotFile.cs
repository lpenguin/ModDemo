using Godot;

namespace ModDemo.Util;

public static class GodotFile
{
    public static string ReadAllText(string path)
    {
        var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        string content = file.GetAsText();
        file.Close();
        return content;
    }

    public static void WriteAllText(string path, string content)
    {
        var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        file.StoreString(content);
        file.Close();
    }
}