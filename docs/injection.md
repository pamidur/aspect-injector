## Aspect Injector Docs

- [<- to contents...](readme.md)

### Injecting Aspects

In the AspectInjector, injection is done by applying the trigger attribute to a target.
Trigger is created by defining .net attribute class and marking it with ```[Injection]``` attribute.

```c#
// defining aspect
[Aspect(Scope.Global)]
class LogAspect {}

// defining a trigger
[Injection(typeof(LogAspect))]
class Log : Attribute {}
//
```

After the trigger is defined you can apply it to the target:

```c#
class TestClass {
    [Log]
    public void DoSomething() {}
}
```

However the trigger is only an "attempt" to inject something into the target. When the trigger is fired for an aspect and the target then it is up to the aspect to decide if anything is injected. **For instance if the aspect effects are configured to deal only with public members and the target doesn't have such then nothing is injected**.

#### Multi-trigger

Triggers can be defined to inject multiple aspects at a time.

```c#
// defining log aspect
[Aspect(Scope.Global)]
class LogAspect {}

// defining measure execution time aspect
[Aspect(Scope.Global)]
class MeasureAspect {}

// defining a trigger
[Injection(typeof(LogAspect))]
[Injection(typeof(MeasureAspect))]
class LogAll : Attribute {}
```

#### Injection propagation

Trigger propagates injection to submembers of a target. This behaviour will be customizable in future versions.

```c#
[Log]
class TestClass {
    public void DoSomething() {}
    public void DoSomethingElse() {}
}

// equals to

class TestClass {
    [Log]
    public void DoSomething() {}
    [Log]
    public void DoSomethingElse() {}
}

// and in case your dll has the only class - equals to
[assembly: Log]

```

#### Triggers and parameters

Triggers can carry parameters which can be read by an Aspect to execute additional logic.

```c#
[Injection(typeof(LogAspect))]
class Log : Attribute {

    public Level Level { get; }

    public Log (LogLevel level){
        Level = level;
    }
}

class TestClass {
    [Log(Level.Warning)]
    public void DoSomething() {}
}
```
