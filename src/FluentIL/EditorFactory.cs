using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.Collections.Generic;
using System.Linq;

namespace FluentIL
{
    public static class EditorFactory
    {
        private static readonly Dictionary<MethodBody, ILProcessor> _processors = new Dictionary<MethodBody, ILProcessor>();
        private static readonly Dictionary<ModuleDefinition, ExtendedTypeSystem> _typeSystems = new Dictionary<ModuleDefinition, ExtendedTypeSystem>();
        private static readonly Dictionary<MethodDefinition, MethodEditor> _methodEditors = new Dictionary<MethodDefinition, MethodEditor>();

        public static ILProcessor GetEditor(this MethodBody mb)
        {
            lock (_processors)
            {
                if (!_processors.TryGetValue(mb, out var result))
                {
                    result = mb.GetILProcessor();
                    _processors.Add(mb, result);
                }

                return result;
            }
        }

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
            foreach (var body in _processors.Keys.Where(b=>b.Method.Module == module))
                body.OptimizeMacros();
        }

        public static void CleanUp(ModuleDefinition module)
        {
            foreach (var body in _processors.Keys.Where(b => b.Method.Module == module).ToList())
            {
                _processors.Remove(body);
                _methodEditors.Remove(body.Method);
            }

            _typeSystems.Remove(module);
        }
    }
}