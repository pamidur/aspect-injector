This package provides simple cache attribute implementation. 
Put ```[MemoryCache]``` attribure on your methods to easily cache them. 

It also works with **void** and **async** methods!

```c#
class TestClass
{
    [MemoryCache(3)] //cache results in memory for 3 sec
    public long Calculate(int a, string b)
    {
        return a + b.GetHashCode() + DateTime.Now.Ticks;
    }
}

public async Task Code()
{
    var target = new TestCalss();
    var expected = target.Calculate(10, "test");
    await Task.Delay(10);
    var result = target.Calculate(10, "test");

    Assert.Equal(expected, result); //equal because the result for input parameters is cached for 3 sec
}
```

Feel free to implement your own cache mechanic by inheriting ```CacheAttribute``` class

```c#
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RedisCacheAttribute : CacheAttribute
{
...
}

```