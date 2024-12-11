using Godot;

namespace ModDemo.Scripting
{
    public class LuaGlobals
    {
        private readonly Node _context;
        private readonly GameServices _services;

        public LuaGlobals(Node context, GameServices services)
        {
            _context = context;
            _services = services;
        }

        [LuaGlobal("DebugPrint")]
        public void DebugPrint(string message)
        {
            GD.Print(message);
        }

        [LuaGlobal("GetScriptParent")]
        public string GetScriptParent()
        {
            return _context.GetParent().GetInstanceId().ToString();
        }

        [LuaGlobal("DestroyObject")]
        public void DestroyObject(string instanceId)
        {
            if (System.UInt64.TryParse(instanceId, out System.UInt64 id))
            {
                if (_context.GetTree().GetRoot().GetNode(id.ToString()) is Node godotObject)
                {
                    godotObject.QueueFree();
                }
            }
            else
            {
                GD.PrintErr($"Invalid instance id: {instanceId}");
            }
        }

        [LuaGlobal("GetObjectPosition")]
        public float[] GetObjectPosition(string instanceId)
        {
            if (System.UInt64.TryParse(instanceId, out System.UInt64 id))
            {
                if (_context.GetTree().GetRoot().GetNode(id.ToString()) is Node3D godotObject)
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

        [LuaGlobal("ShowMessage")]
        public void ShowMessage(string message)
        {
            _services.ModUi.AddMessage(message);
        }

        [LuaGlobal("SetValue")]
        public void SetValue(string key, object value)
        {
            _services.Storage.Set(key, value);
        }

        [LuaGlobal("GetValue")]
        public object? GetValue(string key, object? def = default)
        {
            return _services.Storage.Get(key, def);
        }

        [LuaGlobal("PlayEffect")]
        public void PlayEffect(string effect, float[] position)
        {
            if (position.Length != 3)
            {
                GD.Print("PlayEffect: Invalid position");
                return;
            }
            _services.EffectManager.PlayEffect(effect, 
                new Vector3(position[0], position[1], position[2]));
        }
    }
}