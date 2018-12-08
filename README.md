<img src="https://raw.githubusercontent.com/pamidur/aspect-injector/master/package.png" width="48" align="right"/>Aspect Injector
========================
**Aspect Injector** is a framework for creating and injecting aspects into your .net assemblies.

### Project Status
[![NuGet](https://img.shields.io/nuget/v/AspectInjector.svg)](https://www.nuget.org/packages/AspectInjector)
[![NuGet Pre Release](https://img.shields.io/nuget/vpre/AspectInjector.svg)](https://www.nuget.org/packages/AspectInjector)
[![Build Status](https://travis-ci.org/pamidur/aspect-injector.svg?branch=master)](https://travis-ci.org/pamidur/aspect-injector)

### Download
```bash
> dotnet add package AspectInjector
```

### Features
- Compile-time injection
- Injection Before, After and Around targets
- Injection into Methods, Properties, Events
- Injection Interface implementaions
- Works with any framework/runtime that supports netstandard2.0
- Debugging support
- Roslyn analyzers for your convenience (only c# currently)

Check out [samples](samples) and [docs](docs)

### Requirements
- .NetCore runtime 2.1.6+

### Known Issues
- Due to dotnet nuget [bug](https://github.com/NuGet/Home/issues/6091), you'll need to reference AspectInjector into every project when you want your injections work. They've promised to fix it soon

### Trivia

#### Create aspect:
```C#
[Aspect(Scope.Global)]
[Injection(typeof(LogCall))]
class LogCall : Attribute
{
    [Advice(Kind.Before)]
    public void LogEnter([Argument(Source.Name)] string name)
    {
        Console.WriteLine($"Calling {name}");   //you can debug it	
    }
}
```
#### Use it:
```C#
[LogCall]
public void Calculate() { }
```
