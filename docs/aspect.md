## Aspect Injector Docs

- [<- to contents...](readme.md)

### Defining Aspects

One starts to define an Aspect by applying the ```[Aspect]``` attribute onto a given class. The class decorated with an aspect cannot be ```abstract```, ```static```, and generic. Otherwise you'll get an [error](errors/readme.md).

```c#
[Aspect(Scope.Global)]
class Log
{
}
```

There are currently two scopes in which aspect can operate:

- **Scope.Global** - means aspect will operate as singleton.
- **Scope.PerInstance** - means that every target's class will have its own instance of the aspect. Even if aspect is injected several times into different members of the class, still there will be the only instance of the aspect for this class.

In addition, your aspect can be created either with a parameterless constructor or factory. Make sure the factory class has a method with the proper signature:

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

Next step is to define how your Aspect interacts with injection targets. You can achieve this by using [Mixin](mixin.md) and/or [Advice](advice.md) effects.
