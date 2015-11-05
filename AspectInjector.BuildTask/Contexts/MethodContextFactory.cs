using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AspectInjector.BuildTask.Contexts
{
    internal static class MethodContextFactory
    {
        private static readonly Dictionary<MethodDefinition, TargetMethodContext> Contexts = new Dictionary<MethodDefinition, TargetMethodContext>();

        public static TargetMethodContext GetOrCreateContext(MethodDefinition md)
        {
            lock (Contexts)
            {
                TargetMethodContext result;

                if (!Contexts.TryGetValue(md, out result))
                {
                    if (md.CustomAttributes.HasAttributeOfType<AsyncStateMachineAttribute>() && md.ReturnType != md.Module.TypeSystem.Void)
                        result = new TargetAsyncMethodContext(md);
                    else
                        result = new TargetMethodContext(md);

                    Contexts.Add(md, result);
                }

                return result;
            }
        }
    }
}