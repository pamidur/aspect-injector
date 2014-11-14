using System;
using System.Collections.Generic;

namespace AspectInjector.Snippets
{
    internal static class AspectFactory
    {
        private static readonly Dictionary<Type, object> _aspectsPerTypeCache = new Dictionary<Type, object>();

        public static object GetPerTypeAspect(Type aspectType, Type targetType)
        {
            if (!_aspectsPerTypeCache.ContainsKey(targetType))
            {
                lock (_aspectsPerTypeCache)
                {
                    if (!_aspectsPerTypeCache.ContainsKey(targetType))
                    {
                        _aspectsPerTypeCache[targetType] = Activator.CreateInstance(aspectType);
                    }
                }
            }

            return _aspectsPerTypeCache[targetType];
        }
    }
}