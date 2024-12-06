using Godot;
using ModDemo.Util;
using MoonSharp.Interpreter;
using Script = MoonSharp.Interpreter.Script;

namespace ModDemo.Game.Objects;

public partial class ScriptNode : Node, IDamageHandler
{
    [Export]
    public string Script { get; set; }

    private Script? _script;
    private Closure? _damageHandler;

    public override void _Ready()
    {
        _script = new Script
        {
            Globals =
            {
                ["DebugPrint"] = (string message) => GD.Print(message),
                ["GetScriptParent"] = () => GetScriptParent(),
                ["DestroyObject"] = (string id) => DestroyObject(id)
            }
        };
        _script.DoString(Script);

        if (_script.Globals["Ready"] is Closure closure)
        {
            closure.Call();
        }

        if (_script.Globals["OnDamage"] is Closure damageHandler)
        {
            _damageHandler = damageHandler;
        }
    }

    private string GetScriptParent()
    {
        return GetParent().GetInstanceId().ToString();
    }

    private void DestroyObject(string instanceId)
    {
        if (System.UInt64.TryParse(instanceId, out System.UInt64 id))
        {
            if (InstanceFromId(id) is Node godotObject)
            {
                godotObject.QueueFree();
            }
        }
        else
        {
            GD.PrintErr($"Invalid instance id: {instanceId}");
        }
    }
    public void ApplyDamage(float damage)
    {
        _damageHandler?.Call(damage);
    }
}