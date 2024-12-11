using System;

namespace ModDemo.Scripting;

[AttributeUsage(AttributeTargets.Method)]
public class LuaGlobalAttribute : Attribute
{
    public string Name { get; }

    public LuaGlobalAttribute(string? name = null)
    {
        Name = name ?? string.Empty;
    }
}