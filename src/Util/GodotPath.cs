using System.Collections.Generic;
using Godot;

namespace ModDemo.Util;

public static class GodotPath
{
    public static string Combine(params string[] paths)
    {
        return string.Join("/", paths);
    }

    public static List<string> GetFilesInDirectory(string path)
    {
        var files = new List<string>();
        DirAccess dir = DirAccess.Open(path);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                if (!dir.CurrentIsDir())
                {
                    files.Add(fileName);
                }

                fileName = dir.GetNext();
            }

            dir.ListDirEnd();
        }
        else
        {
            GD.Print($"An error occurred when trying to access the path: {path}");
        }

        return files;
    }

}