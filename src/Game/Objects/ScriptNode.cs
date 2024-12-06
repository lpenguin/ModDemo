using Godot;
using ModDemo.Nodes;
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
    private Closure? _updateHandler;

    public override void _Ready()
    {
        _script = new Script
        {
            Globals =
            {
                ["DebugPrint"] = (string message) => GD.Print(message),
                ["GetScriptParent"] = () => GetScriptParent(),
                ["DestroyObject"] = (string id) => DestroyObject(id),
                ["GetObjectPosition"] = (string id) => GetObjectPosition(id),
                ["ShowMessage"] = (string message) => ModUi.Instance.AddMessage(message),
                ["SetValue"] = (string key, object value) => KeyValuesStorage.Instance.Set(key, value),
                ["GetValue"] = (string key) => KeyValuesStorage.Instance.Get(key),
                ["PlayEffect"] = (string effect, float[] position) => PlayEffect(effect, position),
            }
        };
        _script.DoString(Script);

        GetClosure("Ready")?.Call();
        _damageHandler = GetClosure("OnDamage");
        _updateHandler = GetClosure("Update");
    }

    private void PlayEffect(string effect, float[] position)
    {
        if (position.Length != 3)
        {
            GD.Print("PlayEffect: Invalid position");
            return;
        }
        EffectManager.Instance.PlayEffect(effect, new Vector3(position[0], position[1], position[2]));
    }

    public override void _Process(double delta)
    {
        _updateHandler?.Call(delta);
    }

    private Closure? GetClosure(string name)
    {
        if (_script?.Globals[name] is Closure closure)
        {
            return closure;
        }
        return null;
    }

    private string GetScriptParent()
    {
        return GetParent().GetInstanceId().ToString();
    }

    private float[] GetObjectPosition(string instanceId)
    {
        if (System.UInt64.TryParse(instanceId, out System.UInt64 id))
        {
            if (InstanceFromId(id) is Node3D godotObject)
            {
                return new[]
                {
                    godotObject.Position.X,
                    godotObject.Position.Y,
                    godotObject.Position.Z
                };
            }
        }
        else
        {
            GD.PrintErr($"Invalid instance id: {instanceId}");
        }
        return new float[] { 0, 0, 0 };
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