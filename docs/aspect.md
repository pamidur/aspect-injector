## Aspect Injector Docs
- [How it works](readme.md)
- [Terminology](terminology.md)
- [Defining Aspects](#this) _(on this page)_
- [Injecting Aspects](injection.md)
- [Advice Effect](advice.md)
- [Mixin Effect](mixin.md)

### <a name="this"></a>Defining Aspects
You start define an Aspect by putting ```[Aspect]``` attribute onto class. 
Class defined as an aspect cannot be ```abstract```, ```static```, and generic. Otherwise you'll get an [error](errors/readme.md).
```c#
[Aspect(Scope.Global)]
class Log
{
}
```
There are currently two scopes in which aspect can operate:
- **Scope.Global** - means aspect will operate as singleton. 
- **Scope.PerInstance** - means that every target's class will have its own instance of the aspect. Even if aspect is injected several times into different members of the class, still there will be the only instance of the aspect for this class.

In Addition your aspect can be created either with parameterless constructor or factory. In case you use factory make sure that factory has class has a method with proper signature.
```c#
[Aspect(Scope.Global, Factory = typeof(AspectFactory))]
class Log
{
}

class AspectFactory
{
    public static object GetInstance(Type type)
    {
    }
}
```

Next step is to define how your Aspect interacts with injection targets. You can achieve it by using [Mixin](mixin.md) and/or [Advice](advice.md) effects 
