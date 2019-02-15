using Mono.Cecil;
using Mono.Cecil.Rocks;
using System.Collections.Generic;
using System.Linq;

namespace FluentIL
{
    public static class EditorFactory
    {
        private static readonly Dictionary<ModuleDefinition, ExtendedTypeSystem> _typeSystems = new Dictionary<ModuleDefinition, ExtendedTypeSystem>();
      
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
    }
}