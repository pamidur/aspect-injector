using AspectInjector.Broker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;

namespace Aspects.Cache
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [Injection(typeof(CacheAspect), Inherited = true)]
    public abstract class CacheAttribute : Attribute
    {
        public abstract ObjectCache Cache { get; }

        public abstract CacheItemPolicy Policy { get; }

        public bool PerInstanceCache { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class MemoryCacheAttribute : CacheAttribute
    {
        private static readonly MemoryCache _cache = new MemoryCache("aspect_builtin_memory_cache");

        private readonly uint _seconds;

        public MemoryCacheAttribute(uint seconds, bool perInstanceCache = false)
        {
            _seconds = seconds;
            PerInstanceCache = perInstanceCache;
        }

        public override ObjectCache Cache => _cache;

        public override CacheItemPolicy Policy => new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(_seconds) };
    }


    [Aspect(Scope.Global)]
    public class CacheAspect
    {
        private static readonly object NullMarker = new object();

        [Advice(Kind.Around)]
        public object Handle(
            [Argument(Source.Target)] Func<object[], object> target,
            [Argument(Source.Arguments)] object[] args,
            [Argument(Source.Instance)] object instance,
            [Argument(Source.ReturnType)] Type retType,
            [Argument(Source.Triggers)] Attribute[] triggers
            )
        {
            object result = null;
            var resultFound = false;

            var cacheTriggers = (triggers.OfType<CacheAttribute>()).ToList();
            var useInstance = cacheTriggers.Any(ct => ct.PerInstanceCache);
            var key = useInstance ? GetKey(instance, target.Method, args) : GetKey(target.Method, args);

            foreach (var cache in cacheTriggers.Select(ct => ct.Cache).Distinct())
            {
                var ci = cache.GetCacheItem(key);
                if (ci != null)
                {
                    result = ci.Value;
                    if (result == NullMarker)
                        result = null;

                    resultFound = true;
                    break;
                }
            }

            if (!resultFound)
            {
                result = target(args);
                if (result == null)
                    result = NullMarker;

                foreach (var cache in cacheTriggers)
                    cache.Cache.Set(key, result, cache.Policy);
            }

            return result;
        }

        protected string GetKey(MethodInfo method, IEnumerable<object> args) =>
            $"{method.DeclaringType.FullName.GetHashCode()}-{string.Join("-", args.Select(a => a.GetHashCode()))}";

        protected string GetKey(object instance, MethodInfo method, IEnumerable<object> args) =>
            $"{instance.GetHashCode()}-{method.DeclaringType.FullName.GetHashCode()}-{string.Join("-", args.Select(a => a.GetHashCode()))}";
    }
}
