## Aspect Injector Docs
- [ <- to contents...](readme.md)

### <a name="this"></a>Terminology
We're trying to use the same terminology as other frameworks do. So if are falimial with those, you're familiar with Aspect Injector.

- **Aspect** - class that incapsulates logic some logic. It has _Effects_ which are kind of entrypoints to aspect. [more](aspect.md)

- **Effect** - formal description of how aspect interacts with other classes. There are currencly two types of effect: _Advice_ and _Mixin_. Both can be present within single aspect together and many times. Proper combination of effects allow some complex scenarios.

- **Advice** - a type of effect that describes how method is modified. It can used to inject some code before, after and instead of a method. [more](advice.md)

- **Mixin** - a type of effect that describes how class is modified. It can be used to alter classes implemented interfaces. [more](mixin.md)

- **Trigger** - dotnet attribute that tells AspectInjector which aspect should be injected into target. [more](injection.md)

- **Injection**(or Pointcut) - process of consumption of aspects. Aspect consumer can use injections to leverage aspect's logic. [more](injection.md)
