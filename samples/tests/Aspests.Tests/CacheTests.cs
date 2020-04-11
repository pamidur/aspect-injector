using Aspects.Cache;
using System;
using System.Threading.Tasks;
using Xunit;

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = true)]

namespace Aspests.Tests
{
    public class CacheTests
    {
        class TestCalss
        {
            [MemoryCache(3)]
            public void Do(ref int a)
            {
                a++;
            }

            [MemoryCache(3)]
            public Task DoTask(ref int a)
            {
                a++;
                return Task.CompletedTask;
            }

            [MemoryCache(3)]
            public long Calculate(int a, string b)
            {
                return a + b.GetHashCode() + DateTime.Now.Ticks;
            }

            [MemoryCache(3)]
            public Task<long> CalculateTask(int a, string b)
            {
                return Task.FromResult(a + b.GetHashCode() + DateTime.Now.Ticks);
            }

            [MemoryCache(3)]
            public async Task<long> CalculateTaskAsync(int a, string b)
            {
                return await Task.FromResult(a + b.GetHashCode() + DateTime.Now.Ticks);
            }

            [MemoryCache(3)]
            public static long CalculateStatic(int a, string b)
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
            await Task.Delay(10);
            var result3 = target.Calculate(10, "test");
            await Task.Delay(3000);
            var result4 = target.Calculate(10, "test");

            Assert.Equal(expected, result);
            Assert.Equal(expected, result3);
            Assert.NotEqual(result2, result);
            Assert.NotEqual(result4, result);
        }

        [Fact]
        public async Task Cache_Aspect_Distinct_Instances()
        {
            var target = new TestCalss();
            var target2 = new TestCalss();
            var result1 = target.Calculate(20, "test");
            await Task.Delay(10);
            var result2 = target2.Calculate(20, "test");

            Assert.NotEqual(result1, result2);
        }

        [Fact]
        public async Task Cache_Aspect_Caches_Static_Method_Result()
        {
            var expected = TestCalss.CalculateStatic(30, "test");
            await Task.Delay(10);
            var result = TestCalss.CalculateStatic(30, "test");
            await Task.Delay(10);
            var result2 = TestCalss.CalculateStatic(301, "test1");
            await Task.Delay(10);
            var result3 = TestCalss.CalculateStatic(30, "test");
            await Task.Delay(3000);
            var result4 = TestCalss.CalculateStatic(30, "test");

            Assert.Equal(expected, result);
            Assert.Equal(expected, result3);
            Assert.NotEqual(result2, result);
            Assert.NotEqual(result4, result);
        }

        [Fact]
        public async Task Cache_Aspect_Caches_TaskMethod_Result()
        {
            var target = new TestCalss();
            var expected = await target.CalculateTask(40, "test");
            await Task.Delay(10);
            var result = await target.CalculateTask(40, "test");
            await Task.Delay(10);
            var result2 = await target.CalculateTask(401, "test1");
            await Task.Delay(10);
            var result3 = await target.CalculateTask(40, "test");
            await Task.Delay(3000);
            var result4 = await target.CalculateTask(40, "test");

            Assert.Equal(expected, result);
            Assert.Equal(expected, result3);
            Assert.NotEqual(result2, result);
            Assert.NotEqual(result4, result);
        }

        [Fact]
        public async Task Cache_Aspect_Caches_AsyncTaskMethod_Result()
        {
            var target = new TestCalss();
            var expected = await target.CalculateTaskAsync(50, "test");
            await Task.Delay(10);
            var result = await target.CalculateTaskAsync(50, "test");
            await Task.Delay(10);
            var result2 = await target.CalculateTaskAsync(501, "test1");
            await Task.Delay(10);
            var result3 = await target.CalculateTaskAsync(50, "test");
            await Task.Delay(3000);
            var result4 = await target.CalculateTaskAsync(50, "test");

            Assert.Equal(expected, result);
            Assert.Equal(expected, result3);
            Assert.NotEqual(result2, result);
            Assert.NotEqual(result4, result);
        }

        [Fact]
        public void Cache_Void_Method()
        {
            var target = new TestCalss();
            var a = 1;
            target.Do(ref a);
            Assert.Equal(2, a);

            a = 1;
            target.Do(ref a);
            Assert.Equal(1, a);
        }

        [Fact]
        public async Task Cache_TaskVoid_Method()
        {
            var target = new TestCalss();
            var a = 1;
            await target.DoTask(ref a);
            Assert.Equal(2, a);

            a = 1;
            await target.DoTask(ref a);
            Assert.Equal(1, a);
        }
    }
}
