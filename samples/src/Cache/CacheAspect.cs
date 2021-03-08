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

        /// <summary>
        /// Data is cached PerInstance vs PerType. Default PerInstanceCache = true
        /// </summary>
        public bool PerInstanceCache { get; set; }

        /// <summary>
        /// Cache with highest priority is checked first. 0 - Highest, 255 - Lowest. Default 127
        /// </summary>
        public byte Priority { get; set; } = 127;
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class MemoryCacheAttribute : CacheAttribute
    {
        private static readonly MemoryCache _cache = new MemoryCache("aspect_builtin_memory_cache");

        public MemoryCacheAttribute(uint seconds)
        {
            Policy = new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(seconds) };
        }

        public override ObjectCache Cache => _cache;

        public override CacheItemPolicy Policy { get; }
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

            var cacheTriggers = triggers.OfType<CacheAttribute>().ToList();
            var nonInstanceKey = GetKey(target.Method, args);
            var instanceKey = instance == null ? nonInstanceKey : $"{instance.GetHashCode()}-{nonInstanceKey}";

            foreach (var cacheTrigger in cacheTriggers.OrderBy(c => c.Priority))
            {
                var key = cacheTrigger.PerInstanceCache ? instanceKey : nonInstanceKey;
                var cache = cacheTrigger.Cache;
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
                result = target(args) ?? NullMarker;

                foreach (var cacheTrigger in cacheTriggers)
                {
                    var key = cacheTrigger.PerInstanceCache ? instanceKey : nonInstanceKey;
                    cacheTrigger.Cache.Set(key, result, cacheTrigger.Policy);
                }

            }

            return result;
        }

        private string GetKey(MethodInfo method, IEnumerable<object> args) =>
            $"{method.GetHashCode()}-{string.Join("-", args.Select(a => a.GetHashCode()))}";
    }
}
