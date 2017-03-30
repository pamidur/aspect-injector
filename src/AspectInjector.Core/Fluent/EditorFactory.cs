using AspectInjector.Core.Fluent.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace AspectInjector.Core.Fluent
{
    public static class EditorFactory
    {
        private static readonly Dictionary<MethodBody, ILProcessor> Processors = new Dictionary<MethodBody, ILProcessor>();
        private static readonly Dictionary<ModuleDefinition, ExtendedTypeSystem> TypeSystems = new Dictionary<ModuleDefinition, ExtendedTypeSystem>();
        private static readonly Dictionary<MethodDefinition, MethodEditor> MethodEditors = new Dictionary<MethodDefinition, MethodEditor>();

        public static ILProcessor GetEditor(this MethodBody mb)
        {
            lock (Processors)
            {
                ILProcessor result;

                if (!Processors.TryGetValue(mb, out result))
                {
                    result = mb.GetILProcessor();
                    Processors.Add(mb, result);
                }

                return result;
            }
        }

        public static MethodEditor GetEditor(this MethodDefinition md)
        {
            lock (MethodEditors)
            {
                MethodEditor result;

                if (!MethodEditors.TryGetValue(md, out result))
                {
                    result = new MethodEditor(md);
                    MethodEditors.Add(md, result);
                }

                return result;
            }
        }

        public static ExtendedTypeSystem GetTypeSystem(this ModuleDefinition md)
        {
            lock (TypeSystems)
            {
                ExtendedTypeSystem result;

                if (!TypeSystems.TryGetValue(md, out result))
                {
                    result = new ExtendedTypeSystem(md);
                    TypeSystems.Add(md, result);
                }

                return result;
            }
        }
    }
}