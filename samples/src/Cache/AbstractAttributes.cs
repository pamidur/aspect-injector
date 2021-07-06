using AspectInjector.Broker;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Text.Json;

namespace Aspects.Cache
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [Injection(typeof(CacheAspect), Inherited = true)]
    public abstract class CacheAttribute : Attribute
    {   
        /// <summary>
        /// Cache with highest priority is checked first. 0 - Highest, 255 - Lowest. Default 127
        /// </summary>
        public byte Priority { get; set; } = 127;

        public virtual object Get(string key, Type type, object instance)
        {
            key = instance != null && IsPerInstance() ? FormatInstanceKey(key, instance) : key;
            return GetCacheItem(key, type);
        }

        public virtual void Set(string key, object value, Type type, object instance)
        {
            key = instance != null && IsPerInstance() ? FormatInstanceKey(key, instance) : key;
            SetCacheItem(key, value, type);
        }

        public virtual string FormatInstanceKey(string key, object instance)
        {
            return instance != null && IsPerInstance() ? $"{instance.GetHashCode()}-{key}" : key;
        }

        protected abstract object GetCacheItem(string key, Type type);

        protected abstract void SetCacheItem(string key, object value, Type type);

        protected abstract bool IsPerInstance();
    }

    public abstract class MemoryCacheBaseAttribute : CacheAttribute
    {
        protected override object GetCacheItem(string key, Type type)
        {
            return Cache.Get(key);
        }
        protected override void SetCacheItem(string key, object value, Type type)
        {
            Cache.Set(key, value, Policy);
        }

        protected override bool IsPerInstance()
        {
            return PerInstanceCache;
        }

        public abstract IMemoryCache Cache { get; }

        public abstract MemoryCacheEntryOptions Policy { get; }

        /// <summary>
        /// Data is cached PerInstance vs PerType. Default PerInstanceCache = true
        /// </summary>
        public bool PerInstanceCache { get; set; }
    }

    public abstract class DistributedCacheBaseAttribute : CacheAttribute
    {
        protected override object GetCacheItem(string key, Type type)
        {            
            return Deserialize(Cache.Get(key), type);
        }

        protected override void SetCacheItem(string key, object value, Type type)
        {            
            Cache.Set(key, Serialize(value, type), Policy);
        }

        protected override bool IsPerInstance()
        {
            return PerInstanceCache;
        }

        protected virtual byte[] Serialize(object value, Type type)
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, type);
        }

        protected virtual object Deserialize(byte[] data, Type type)
        {
            if (data == null || data.Length == 0) return null;
            return JsonSerializer.Deserialize(data, type);
        }       

        public abstract IDistributedCache Cache { get; }
        public abstract DistributedCacheEntryOptions Policy { get; }

        /// <summary>
        /// Data is cached PerInstance vs PerType. Default PerInstanceCache = true
        /// </summary>
        public bool PerInstanceCache { get; set; }
    }
}
