## Aspect Injector Docs
- [ <- to contents...](readme.md)

### <a name="this"></a>Advice Effect
Advice effect is a code execution that can be injected into method and by extension into properties and events as they use methods under the hood.

Advice is defined as a method within the aspect class and marked with an ```[Advice]``` attribute. This attribute has the only required parameter that determines the kind of the advice.
Advice method can also accept various arguments. See [Advice Effect Arguments](advicearguments.md) to find out more.
```c#
class LogAspect {
    [Advice(Kind.Before)]
    public void LogEnter() {
        Console.WriteLine("Entering ...");
    }

    [Advice(Kind.After)]
    public void LogExit() {
        Console.WriteLine("Leaving ...");
    }

    [Advice(Kind.Around)]
    public object LogAndMeasureTimings(...) {
        ...
    }
}
```
There are three kind of advice at the moment:
- **Before** - the code is injected before the method begins. Special case for constructors - the code is injected before the constructor begins but after the call to base class .ctor is done.
- **After** - the code is injected after the method ends.
- **Around** - the code is executed instead of the target method. The call to original method is optional and up to advice definition. Refer to [Advice Effect Arguments](advicearguments.md) find out how to make original method call. This advice however has to return an object that will casted to target method's return type. Even if the target method's return type is ```void```, the advice have to return at least ```null```. Special case for constructors - Around advice cannot be applied around constructors.

#### Advice targets
Advice can target certain type of members via the second parameter of an ```[Advice]``` attribute.
```c#
class LogAspect {
    [Advice(Kind.Before, Targets = Target.Public | Target.Setter)]
    public void LogEnter() {
        Console.WriteLine("Entering public property setter...");
    }
}
```
The ```Target``` ennumeration has the following options:

Complex values:
- **Any** - All members. (AnyMember + AnyAccess + AnyScope) (Default value)
- **AnyMember** = Members of any type. (Default member type value)
- **AnyAccess** - Members of any access. (Default member access value)
- **AnyScope** = Members of any scope. (Default member scope value) 

Access values:
- **Private** - Private members.
- **Internal** - Internal members.
- **Protected** - Protected members.
- **ProtectedInternal** - Protected internal members (Protected OR Internal).
- **ProtectedPrivate** - Protected AND internal members.
- **Public** - Public members.

Scope values:
- **Static** - Static members.
- **Instance** - Non static members.

Type values:

- **Constructor** - Constructors.
- **Method** - Methods.
- **Getter** - Property getters.
- **Setter** - Property setters.
- **EventAdd** - Event subscribe handlers.
- **EventRemove** - Event unsubscribe handlers.
