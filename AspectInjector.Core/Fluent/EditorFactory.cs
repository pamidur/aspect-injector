using AspectInjector.Core.Fluent;
using AspectInjector.Core.Fluent.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace AspectInjector.Core.Fluent
{
    public class EditorFactory
    {
        private readonly Dictionary<MethodBody, ILProcessor> Processors = new Dictionary<MethodBody, ILProcessor>();
        private readonly Dictionary<ModuleDefinition, EditorContext> EditorContexts = new Dictionary<ModuleDefinition, EditorContext>();
        private readonly Dictionary<MethodDefinition, MethodEditor> MethodEditors = new Dictionary<MethodDefinition, MethodEditor>();
        public string Prefix { get; private set; }

        public EditorFactory(string prefix)
        {
            Prefix = prefix;
        }

        public ILProcessor GetProcessor(MethodBody mb)
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

        public MethodEditor GetEditor(MethodDefinition md)
        {
            lock (MethodEditors)
            {
                MethodEditor result;

                if (!MethodEditors.TryGetValue(md, out result))
                {
                    //if (md.CustomAttributes.HasAttributeOfType<AsyncStateMachineAttribute>() /* && md.ReturnType != md.Module.TypeSystem.Void*/)
                    //    result = new TargetAsyncMethodContext(md, ModuleContext.GetOrCreateContext(md.Module));
                    //else
                    result = new MethodEditor(GetContext(md.Module), md);

                    MethodEditors.Add(md, result);
                }

                return result;
            }
        }

        public EditorContext GetContext(ModuleDefinition md)
        {
            lock (EditorContexts)
            {
                EditorContext result;

                if (!EditorContexts.TryGetValue(md, out result))
                {
                    result = new EditorContext(md, this);
                    EditorContexts.Add(md, result);
                }

                return result;
            }
        }
    }
}