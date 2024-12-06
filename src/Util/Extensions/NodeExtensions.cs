using Godot;

namespace ModDemo.Util.Extensions;

public static class NodeExtensions
{
    public static bool TryGetChild<T>(this Node node, out T? child)
    {
        child = default;
        foreach (Node childNode in node.GetChildren())
        {
            if (childNode is T tChild)
            {
                child = tChild;
                return true;
            }
        }
        return false;
    }
}