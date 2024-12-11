using Godot;
using ModDemo.Nodes;
using ModDemo.Scripting;

namespace ModDemo.Game.Objects
{
    public partial class ScriptNode : Node, IDamageHandler
    {
        [Export]
        public string Script { get; set; }

        private LuaScript? _luaScript;
        private LuaGlobals? _luaGlobals;
        private readonly GameServices _services = new();

        public override void _Ready()
        {
            _luaGlobals = new LuaGlobals(this, _services);
            _luaScript = new LuaScript(Script, _luaGlobals);
            _luaScript.Initialize();
        }

        public override void _Process(double delta)
        {
            _luaScript?.Update(delta);
        }

        public void ApplyDamage(float damage)
        {
            _luaScript?.HandleDamage(damage);
        }
    }
}