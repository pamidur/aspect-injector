using System;
using System.Collections.Generic;
using System.Threading;

namespace AspectInjector.Snippets
{
    internal static class AspectFactory
    {
        public static Func<Type, object, Type, int, object> GetOrCreateAspectFunc;
        public static Action<object> DisposeAspectFunc;

        static AspectFactory()
        {
            SetupLocalFactories();
            SetupDependencies();
        }

        private static void SetupDependencies()
        {
            return;
        }

        private static void SetupLocalFactories()
        {
            GetOrCreateAspectFunc = GetOrCreateAspectDefault;
            DisposeAspectFunc = DisposeAspectDefault;
        }

        private static void DisposeAspectDefault(object aspect)
        {
            
        }

        private readonly static Dictionary<Type, object> AspectCache = new Dictionary<Type, object>();

        private static object GetOrCreateAspectDefault(Type aspectType, object target, Type targetType, int scope)          
        {
            if (!AspectCache.ContainsKey(targetType))
            {
                lock (AspectCache)
                {
                    if (!AspectCache.ContainsKey(targetType))
                    {
                        AspectCache[targetType] = Activator.CreateInstance(aspectType);
                    }
                }
            }

            return AspectCache[targetType];
        }

        public static T GetOrCreateAspect<T>(object target, Type targetType, int scope)
        {
            return (T)GetOrCreateAspectFunc(typeof(T), target, targetType, scope);
        }
    }
}
