using System;
using System.Collections.Generic;
using Godot;

namespace ModDemo.Util;

public static class GodotScript
{
    private static readonly Dictionary<Type, Variant> ScriptCache = new();
    
    public static T AttachScript<T>(Node node)
        where T : GodotObject
    {
        var scriptType = typeof(T);
        if (!ScriptCache.TryGetValue(scriptType, out Variant script))
        {
            var tempInstance = Activator.CreateInstance<T>();
            script = tempInstance.GetScript();
            tempInstance.Free();
            ScriptCache[scriptType] = script;
        }
        
        ulong instanceId = node.GetInstanceId();
        node.SetScript(script);
        return (T)GodotObject.InstanceFromId(instanceId)!;
    }
}