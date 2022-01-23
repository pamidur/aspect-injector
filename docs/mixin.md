## Aspect Injector Docs

- [<- to contents...](readme.md)

### <a name="this"></a>Mixins

**Mixins** is a powerfull feature that enables developers to add new logic/properties to an object by automatically implementing interfaces.
Aspect containing a mixin will create members in target similar to interface's and proxy them back to the aspect.

Consider scnerio where you want to add a property to a target:
```c#
public class Target
{
  public void Do() {}
}
```

For this you need to create an interface that describes features you want to add:
```c#
public interface IHaveProperty
{
  string Data { get; set; }
}
```

then we will create an aspect:
```c#
[Aspect(Scope.Global)]
[Injection(typeof(MyAspect))]
[Mixin(typeof(IHaveProperty))]
public class MyAspect: Attribute, IHaveProperty 
{
  public string Data { get; set; }
}
```

And finally if apply this new aspect-attribute to `Target` will make look like (after compilation):
```c#
[MyAspect]
public class Target : IHaveProperty
{
  string IHaveProperty.Data
  {
    get
    {
      return ((IHaveProperty)My1Aspect.__a$_instance).Data;
    }
    set
    {
      ((IHaveProperty)My1Aspect.__a$_instance).Data = value;
    }
  }

  public void Do()
  {
  }
}
```

Note that it isn't necessary to apply aspect to `Target` class itself, it is enough to apply it any member, like this:
```c#
public class Target
{
  [MyAspect]
  public void Do() {}
}
```
