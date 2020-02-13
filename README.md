<img src="https://raw.githubusercontent.com/pamidur/aspect-injector/master/package.png" width="48" align="right"/>Aspect Injector
========================
**Aspect Injector** is an attribute-based framework for creating and injecting aspects into your .net assemblies.

### Project Status
[![Nuget](https://img.shields.io/nuget/v/AspectInjector?label=stable&logo=nuget)](https://www.nuget.org/packages/AspectInjector)
[![Nuget Pre](https://img.shields.io/nuget/vpre/AspectInjector?label=latest&logo=nuget)](https://www.nuget.org/packages/AspectInjector)
![GitHub (Pre-)Release Date](https://img.shields.io/github/release-date-pre/pamidur/aspect-injector)
![GitHub last commit](https://img.shields.io/github/last-commit/pamidur/aspect-injector)

[![Build Status](https://travis-ci.org/pamidur/aspect-injector.svg?branch=master)](https://travis-ci.org/pamidur/aspect-injector)
[![Samples Status](https://img.shields.io/github/workflow/status/pamidur/aspect-injector/Samples?label=samples%20build)](https://github.com/pamidur/aspect-injector/commits/master)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/d9c91dfb194e4c669b0a98031c59b26e)](https://www.codacy.com/manual/agulyj/aspect-injector?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=pamidur/aspect-injector&amp;utm_campaign=Badge_Grade)

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
- For analyzers to work in VSCode, don't forget to enable ```"omnisharp.enableRoslynAnalyzers": true``` 
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

### Simple advice

#### Create an aspect with simple advice:
```C#
[Aspect(Scope.Global)]
[Injection(typeof(LogCall))]
public class LogCall : Attribute
{
    [Advice(Kind.Before)] // you can have also After (async-aware), and Around(Wrap/Instead) kinds
    public void LogEnter([Argument(Source.Name)] string name)
    {
        Console.WriteLine($"Calling '{name}' method...");   //you can debug it	
    }
}
```
#### Use it:
```C#
[LogCall]
public void Calculate() 
{ 
    Console.WriteLine("Calculated");
}

Calculate();
```
#### Result:
```bash
$ dotnet run
Calling 'Calculate' method...
Calculated
```

### Simple mixin

#### Create an aspect with mixin:
```C#
public interface IInitializable
{
    void Init();
}

[Aspect(Scope.PerInstance)]
[Injection(typeof(Initializable))]
[Mixin(typeof(IInitializable))]
public class Initializable : IInitializable, Attribute
{
    public void Init()
    {
        Console.WriteLine("Initialized!");
    }
}
```
#### Use it:
```C#
[Initializable]
public class Target
{ 
}

var target = new Target() as IInitializable;
target.Init();
```
#### Result:
```bash
$ dotnet run
Initialized!
```
