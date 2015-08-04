using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using System.Collections.Generic;

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
                    if (md.ReturnType.IsTypeOf(typeof(System.Threading.Tasks.Task)))
                        result = new TargetAsyncMethodContext(md);
                    else
                        result = new TargetMethodContext(md);

                    result.Init();

                    Contexts.Add(md, result);
                }

                return result;
            }
        }
    }
}
