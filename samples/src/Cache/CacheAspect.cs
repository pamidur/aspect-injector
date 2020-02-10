using AspectInjector.Broker;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Aspects.Cache
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [Injection(typeof(CacheAspect), Inherited = true)]
    public abstract class CacheAttribute : Attribute
    {
        public abstract ObjectCache Cache { get; }

        public abstract CacheItemPolicy Policy { get; }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class MemoryCacheAttribute : CacheAttribute
    {
        private static readonly MemoryCache _cache = new MemoryCache("aspect_builtin_memory_cache");
        private readonly uint _seconds;

        public MemoryCacheAttribute(uint seconds)
        {
            _seconds = seconds;
        }

        public override ObjectCache Cache => _cache;
        public override CacheItemPolicy Policy => new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(_seconds) };
    }


    [Aspect(Scope.Global)]
    public class CacheAspect
    {
        private static readonly Type _voidTaskResult = Task.FromException(new Exception()).GetType();

        [Advice(Kind.Around)]
        public object Handle(
            [Argument(Source.Target)] Func<object[], object> target,
            [Argument(Source.Arguments)] object[] args,
            [Argument(Source.Name)] string name,
            [Argument(Source.ReturnType)] Type retType,
            [Argument(Source.Triggers)] Attribute[] triggers
            )
        {
            object result = null;
            var resultFound = false;

            var cacheTriggers = triggers.OfType<CacheAttribute>();
            var key = GetKey(target.Method, args);

            foreach (var cache in cacheTriggers.Select(ct => ct.Cache).Distinct())
            {
                var ci = cache.GetCacheItem(key);
                if (ci != null)
                {
                    result = ci.Value;
                    resultFound = true;
                    break;
                }
            }

            if (!resultFound)
            {
                result = target(args);

                foreach (var cache in cacheTriggers)
                    cache.Cache.Set(key, result, cache.Policy);
            }

            return result;
        }

        private string GetKey(MethodInfo method, object[] args) => $"{method.ToString()}{args.Select(a => a.GetHashCode()).Sum()}";

        private bool IsVoid(Type type) => type == typeof(void) || type == typeof(Task) || type == _voidTaskResult;
    }
}
