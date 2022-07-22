using AspectInjector.Broker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aspects.Cache
{
    [Aspect(Scope.Global)]
    public class CacheAspect
    {
        private static readonly object NullMarker = new { __is_null = "$_is_null" };

        [Advice(Kind.Around)]
        public object Handle(
            [Argument(Source.Target)] Func<object[], object> target,
            [Argument(Source.Arguments)] object[] args,
            [Argument(Source.Instance)] object instance,
            [Argument(Source.ReturnType)] Type retType,
            [Argument(Source.Triggers)] Attribute[] triggers
            )
        {
            retType = retType == typeof(void) ? typeof(object) : retType;

            object result = null;
            var resultFound = false;

            var cacheTriggers = triggers.OfType<CacheAttribute>().Distinct().OrderBy(c => c.Priority).ToList();
            var key = GetKey(target.Method, args);


            foreach (var cacheTrigger in cacheTriggers)
            {
                var ci = cacheTrigger.Get(key, retType, instance);
                if (ci != null)
                {
                    result = ci;
                    if (result == NullMarker)
                        result = null;

                    resultFound = true;
                    break;
                }
            }

            if (!resultFound)
            {
                result = target(args) ?? NullMarker;

                foreach (var cacheTrigger in cacheTriggers)
                {
                    cacheTrigger.Set(key, result, retType, instance);
                }
            }
            if (result == NullMarker) return null;
            return result;
        }

        protected string GetKey(MethodInfo method, IEnumerable<object> args) =>
            $"{method.GetHashCode()}-{string.Join("-", args.Select(a => a.GetHashCode()))}";
    }
}
