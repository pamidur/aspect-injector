## Aspect Injector Docs
- [How it works](#this) _(on this page)_
- [Terminology](terminology.md)
- [Defining Aspects](aspect.md)
- [Injecting Aspects](injection.md)
- [Advice Effect](advice.md)
- [Advice Effect Arguments](advicearguments.md)
- [Mixin Effect](mixin.md)

### <a name="this"></a>How it works
AspectInjector is compile-time AOP framework which means that all required work is done in compile time.
For example before compilation your code looks like:
```c#
[Aspect(Scope.Global)]
[Injection(typeof(Log))]
class Log : Attribute
{
    [Advice(Kind.Before, Targets = Target.Method)]
    public void OnEntry([Argument(Source.Name)] string name)
    {
        Console.WriteLine($"Entering method {name}");
    }
}

class TestClass
{
    [Log]
    public void Do()
    {
        Console.WriteLine($"Done");
    }
}
```
Then when you hit F5, we pick up from there and change your assembly a bit, so it actually works like this:
```c#
[Aspect(Scope.Global)]
[Injection(typeof(Log))]
class Log : Attribute
{
    public static readonly Log __a$_instance;

    [Advice(Kind.Before, Targets = Target.Method)]
    public void OnEntry([Argument(Source.Name)] string name)
    {
        Console.WriteLine($"Entering method {name}");
    }

    static Log()
    {
        __a$_instance = new Log();
    }
}

internal class TestClass
{
    [Log]
    public void Do()
    {
        Log.__a$_instance.OnEntry("Do");
        Console.WriteLine("Done");
    }
}
```
Thus there is no performance hit often experienced with runtime AOP frameworks
