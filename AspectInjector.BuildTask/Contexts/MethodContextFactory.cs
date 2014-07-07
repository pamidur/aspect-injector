using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.BuildTask.Contexts
{
    static class MethodContextFactory
    {
        private static readonly Dictionary<MethodDefinition, TargetMethodContext> contexts = new Dictionary<MethodDefinition, TargetMethodContext>();

        public static TargetMethodContext GetOrCreateContext(MethodDefinition md)
        {
            lock (contexts)
            {
                TargetMethodContext result;

                if (!contexts.TryGetValue(md, out result))
                {
                    result = new TargetMethodContext(md);
                    contexts.Add(md, result);
                }

                return result;
            }            
        }
    }
}
