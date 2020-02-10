using Aspects.Cache;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Aspests.Tests
{
    public class CacheTests
    {
        class TestCalss
        {
            [MemoryCache(10)]
            public long Calculate(int a, string b)
            {
                return a + b.GetHashCode() + DateTime.Now.Ticks;
            }
        }

        [Fact]
        public async Task Cache_Aspect_Caches_Method_Result()
        {
            var target = new TestCalss();
            var expected = target.Calculate(10, "test");
            await Task.Delay(10);
            var result = target.Calculate(10, "test");
            await Task.Delay(10);
            var result2 = target.Calculate(101, "test1");

            Assert.Equal(expected, result);
            Assert.NotEqual(result2, result);
        }
    }
}
