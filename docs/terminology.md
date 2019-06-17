## Aspect Injector Docs

- [<- to contents...](readme.md)

### Terminology

We're trying to use the same terminology as other frameworks. So, if you are familiar with those, you're familiar with Aspect Injector.

- **Aspect** - class that encapsulates logic some logic. It has _Effects_ which are kind of entry points to aspect. [more](aspect.md)
- **Effect** - formal description of how aspect interacts with other classes. There are currently two types of effect: _Advice_ and _Mixin_. Both can be present within single aspect together and many times. By applying deliberate combinations of effects, it allows for some complex scenarios.
- **Advice** - a type of effect that describes how method is modified. It can be used to inject some code before, after and/ir instead of a method. [more](advice.md)
- **Mixin** - a type of effect that describes how class is modified. It can be used to alter a class's implemented interfaces. [more](mixin.md)
- **Trigger** - dotnet attribute that tells AspectInjector which aspect should be injected into target. [more](injection.md)
- **Injection** (or Pointcut) - process of consumption of aspects. Aspect consumer can use injections to leverage aspect's logic. [more](injection.md)
