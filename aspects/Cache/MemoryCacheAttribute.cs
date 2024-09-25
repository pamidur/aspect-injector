using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;

namespace Aspects.Cache
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class MemoryCacheAttribute : MemoryCacheBaseAttribute
    {
        private static readonly MemoryCache _cache = new  MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));

        private readonly uint _seconds;

        public MemoryCacheAttribute(uint seconds, bool perInstanceCache = false)
        {
            _seconds = seconds;
            PerInstanceCache = perInstanceCache;
        }

        public override IMemoryCache Cache => _cache;

        public override MemoryCacheEntryOptions Policy => new MemoryCacheEntryOptions() { AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(_seconds) };
    }
}
