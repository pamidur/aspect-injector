## Aspect Injector Docs

- [<- to contents...](readme.md)

### <a name="this"></a>Advice Effect Arguments

Advice method can accept various arguments that represent information about target and triggers and can be used in complex scenarios.

#### Name

**Name** argument carries a name of a target. If the target is a method, then the name will be the name of the method. If injection is applied to a whole property or an event then the name will be the name of the property or the event.

```c#
class LogAspect {
    [Advice(Kind.Before, Targets = Target.Method)]
    public void LogEnter([Argument(Source.Name)] string name)
    {
        Console.WriteLine($"Entering method '{name}'.");
    }
}
```

#### Type

**Type** argument carries a type that contains the target method.

```c#
class CountCreationAspect {
    private int _count = 0;
    [Advice(Kind.Before, Targets = Target.Constructor)]
    public void LogEnter([Argument(Source.Type)] Type type)
    {
        Console.WriteLine($"Instance of type {type.Name} created {++_count} times.");
    }
}
```

#### Instance

**Type** argument has a reference to the instance that owns target method. Note that the reference can be ```null``` when the target method is static.

```c#
class FreezableAspect {
    [Advice(Kind.Before, Targets = Target.Public | Target.Setter)]
    public void CheckIfFrozen([Argument(Source.Instance)] object instance)
    {
        if (instance is IFreezable freezable && freezable.IsFrozen)
            throw new InvalidOperationException("Attempt to modify frozen object.");
    }
}
```

#### Metadata (former Method)

**Metadata** argument refers to reflection metadata of a target method.
