<img src="https://raw.githubusercontent.com/pamidur/aspect-injector/master/package.png" width="48" align="right"/>Aspect Injector
========================
**Aspect Injector** is an attribute-based framework for creating and injecting aspects into your .net assemblies.

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
- Injecting Before, After and Around targets
- Injecting into Methods, Properties, Events
- Injecting Interface implementaions
- Supports any project that can reference **netstandard2.0** libraries
- Debugging support
- Roslyn analyzers for your convenience (only c# currently)

Check out [samples](samples) and [docs](docs)

### Requirements
- .NetCore runtime 2.1.6+

### Known Issues / Limitations
- Unsafe methods are not supported and are silently ignored.
- ~~You cannot inject code around constructors. Such attempts are silently ignored.~~ You can since **2.2.1**!
- Until Nuget v5 you need to refrecence AspectInjector into every project in your solution.
Thus, 
``` 
    if VisualStudio >= 2019 && CoreSDK >= 2.1.602
        no worries about references
    else 
        reference AspectInjector directly to projects where aspects are defined or used
```

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
