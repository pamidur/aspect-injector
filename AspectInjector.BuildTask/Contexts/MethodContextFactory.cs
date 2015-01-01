using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    result = new TargetMethodContext(md);
                    Contexts.Add(md, result);
                }

                return result;
            }            
        }
    }
}
