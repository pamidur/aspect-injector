using Mono.Cecil;
using Mono.Cecil.Rocks;
using System.Collections.Generic;
using System.Linq;

namespace FluentIL
{
    public static class EditorFactory
    {
        private static readonly Dictionary<ModuleDefinition, ExtendedTypeSystem> _typeSystems = new Dictionary<ModuleDefinition, ExtendedTypeSystem>();
        private static readonly Dictionary<MethodDefinition, MethodEditor> _methodEditors = new Dictionary<MethodDefinition, MethodEditor>();

        public static MethodEditor GetEditor(this MethodDefinition md)
        {
            lock (_methodEditors)
            {
                if (!_methodEditors.TryGetValue(md, out var result))
                {
                    result = new MethodEditor(md);
                    _methodEditors.Add(md, result);
                }

                return result;
            }
        }

        public static ExtendedTypeSystem GetTypeSystem(this ModuleDefinition md)
        {
            lock (_typeSystems)
            {
                if (!_typeSystems.TryGetValue(md, out var result))
                {
                    result = new ExtendedTypeSystem(md);
                    _typeSystems.Add(md, result);
                }

                return result;
            }
        }

        public static void Optimize(ModuleDefinition module)
        {
            foreach (var method in _methodEditors.Keys.Where(b=>b.Module == module))
                method.Body.OptimizeMacros();
        }

        public static void CleanUp(ModuleDefinition module)
        {
            foreach (var method in _methodEditors.Keys.Where(b => b.Module == module).ToList())            
                _methodEditors.Remove(method);            

            _typeSystems.Remove(module);
        }
    }
}