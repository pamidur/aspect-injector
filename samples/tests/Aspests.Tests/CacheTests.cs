using Aspects.Cache;
using System;
using System.Threading.Tasks;
using Xunit;

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = true)]

namespace Aspests.Tests
{
    public class CacheTests
    {
        class TestClass
        {
            [MemoryCache(3, PerInstanceCache = false)]
            public int? Nullable(bool ok)
            {
                if (ok) return 1;
                return null;
            }

            [MemoryCache(3, PerInstanceCache = true)]
            public int? NullablePerInstance(bool ok)
            {
                if (ok) return 1;
                return null;
            }

            [MemoryCache(3, PerInstanceCache = false)]
            public void Do(ref int a)
            {
                a++;
            }

            [MemoryCache(3, PerInstanceCache = false)]
            public Task DoTask(ref int a)
            {
                a++;
                return Task.CompletedTask;
            }

            [MemoryCache(3, PerInstanceCache = false)]
            public long Calculate(int a, string b)
            {
                return a + b.GetHashCode() + DateTime.Now.Ticks;
            }

            [MemoryCache(3, PerInstanceCache = true)]
            public long CalculatePerInstance(int a, string b)
            {
                return a + b.GetHashCode() + DateTime.Now.Ticks;
            }

            [MemoryCache(3, PerInstanceCache = false)]
            public long Calculate(int a, int b)
            {
                return a + b + DateTime.Now.Ticks;
            }

            [MemoryCache(3, PerInstanceCache = false)]
            public Task<long> CalculateTask(int a, string b)
            {
                return Task.FromResult(a + b.GetHashCode() + DateTime.Now.Ticks);
            }

            [MemoryCache(3, PerInstanceCache = false)]
            public async Task<long> CalculateTaskAsync(int a, string b)
            {
                return await Task.FromResult(a + b.GetHashCode() + DateTime.Now.Ticks);
            }

            [MemoryCache(3, PerInstanceCache = false)]
            public static long CalculateStatic(int a, string b)
            {
                return a + b.GetHashCode() + DateTime.Now.Ticks;
            }
        }

        [Fact]
        public async Task Cache_Aspect_Caches_Method_Result()
        {
            var target = new TestClass();
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
            var target = new TestClass();
            var target2 = new TestClass();
            var result1 = target.Calculate(20, "test");
            await Task.Delay(10);
            var result2 = target2.Calculate(20, "test");

            Assert.Equal(result1, result2);
        }

        [Fact]
        public async Task Cache_Aspect_Distinct_Instances_PerInstance()
        {
            var target = new TestClass();
            var target2 = new TestClass();
            var result1 = target.CalculatePerInstance(20, "test");
            await Task.Delay(10);
            var result2 = target2.CalculatePerInstance(20, "test");

            Assert.NotEqual(result1, result2);
        }

        [Fact]
        public async Task Cache_Aspect_Caches_Static_Method_Result()
        {
            var expected = TestClass.CalculateStatic(30, "test");
            await Task.Delay(10);
            var result = TestClass.CalculateStatic(30, "test");
            await Task.Delay(10);
            var result2 = TestClass.CalculateStatic(301, "test1");
            await Task.Delay(10);
            var result3 = TestClass.CalculateStatic(30, "test");
            await Task.Delay(3000);
            var result4 = TestClass.CalculateStatic(30, "test");

            Assert.Equal(expected, result);
            Assert.Equal(expected, result3);
            Assert.NotEqual(result2, result);
            Assert.NotEqual(result4, result);
        }

        [Fact]
        public async Task Cache_Aspect_Caches_TaskMethod_Result()
        {
            var target = new TestClass();
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
            var target = new TestClass();
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
        public void Cache_Nullable_Method()
        {
            var target = new TestClass();
            
            var i = target.Nullable(true);
            Assert.Equal(1, i);

            i = target.Nullable(false);
            Assert.Null(i);

            i = target.NullablePerInstance(true);
            Assert.Equal(1, i);
            
            i = target.NullablePerInstance(false);
            Assert.Null(i);
        }

        [Fact]
        public void Cache_Void_Method()
        {
            var target = new TestClass();
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
            var target = new TestClass();
            var a = 1;
            await target.DoTask(ref a);
            Assert.Equal(2, a);

            a = 1;
            await target.DoTask(ref a);
            Assert.Equal(1, a);
        }

        [Fact]
        public async Task Cache_Aspect_OppositeArguments()
        {
            var target = new TestClass();
            var result1 = target.Calculate(20, 5);
            await Task.Delay(10);
            var result2 = target.Calculate(5, 20);

            Assert.NotEqual(result1, result2);
        }
    }
}
