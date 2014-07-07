using System;
using System.Collections.Generic;

namespace AspectInjector.BuildTask.Snippets
{
    internal static class AspectFactory
    {
        private readonly static Dictionary<Type, object> AspectCache = new Dictionary<Type, object>();

        public static T GetOrCreateAspect<T>(object target, Type targetType, object scope)
            where T : class
        {
            if (!AspectCache.ContainsKey(targetType))
            {
                lock (AspectCache)
                {
                    if (!AspectCache.ContainsKey(targetType))
                    {
                        AspectCache[targetType] = Activator.CreateInstance<T>();
                    }
                }
            }

            return (T)AspectCache[targetType];
        }
    }
}
