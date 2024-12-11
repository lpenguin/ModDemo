using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter;

namespace ModDemo.Scripting
{
    public class LuaScript
    {
        private readonly Script _script;
        private readonly Dictionary<string, Closure> _closures = new();
        private readonly LuaGlobals _globals;

        public LuaScript(string scriptContent, LuaGlobals globals)
        {
            _globals = globals;
            _script = new Script();
            
            RegisterGlobals();
            _script.DoString(scriptContent);
        }

        private void RegisterGlobals()
        {
            var methods = typeof(LuaGlobals)
                .GetMethods()
                .Where(m => m.GetCustomAttribute<LuaGlobalAttribute>() != null);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<LuaGlobalAttribute>()!;
                var name = !string.IsNullOrEmpty(attr.Name) ? attr.Name : method.Name;
                
                var del = Delegate.CreateDelegate(
                    method.CreateDelegateType(),
                    _globals,
                    method);

                _script.Globals[name] = del;
            }
        }

        public void Initialize()
        {
            GetClosure("Ready")?.Call();
        }

        public void Update(double delta)
        {
            GetClosure("Update")?.Call(delta);
        }

        public void HandleDamage(float damage)
        {
            GetClosure("OnDamage")?.Call(damage);
        }

        private Closure? GetClosure(string name)
        {
            if (_closures.TryGetValue(name, out var cached))
            {
                return cached;
            }

            if (_script.Globals[name] is Closure closure)
            {
                _closures[name] = closure;
                return closure;
            }

            return null;
        }
    }
}