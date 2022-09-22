![image](https://user-images.githubusercontent.com/4095184/155636545-94ba7c67-277f-46d7-84e3-b750ff341769.png)

<h3><span style="color: #ff0000;"><strong>I have never asked for any donations, but today, I ask you, please, consider donating Ukrainian Army.<br /></strong></span></h3>
<h3><span style="color: #ff0000;"><strong>You can find official ways to do it <a href="https://bank.gov.ua/en/news/all/natsionalniy-bank-vidkriv-spetsrahunok-dlya-zboru-koshtiv-na-potrebi-armiyi">here</a> or you can donate to the biggest charity organization <a href="https://www.comebackalive.in.ua/">here</a></strong></span></h3>
<h3><span style="color: #ff0000;"><strong>People need to be alive to create open source projects!</strong></span></h3>



 
 
_______________________________
_______________________________
_______________________________

-------------------------------
 
 

<img src="https://raw.githubusercontent.com/pamidur/aspect-injector/master/package.png" width="48" align="right"/>Aspect Injector
========================
**Aspect Injector** is an attribute-based framework for creating and injecting aspects into your .net assemblies.

### Project Status
[![Nuget](https://img.shields.io/nuget/v/AspectInjector?label=stable&logo=nuget&style=flat-square)](https://www.nuget.org/packages/AspectInjector)
[![Nuget Pre](https://img.shields.io/nuget/vpre/AspectInjector?label=latest&logo=nuget&style=flat-square)](https://www.nuget.org/packages/AspectInjector)
[![Nuget](https://img.shields.io/nuget/dt/AspectInjector?style=flat-square)](https://www.nuget.org/packages/AspectInjector)

![GitHub (Pre-)Release Date](https://img.shields.io/github/release-date-pre/pamidur/aspect-injector?style=flat-square)
![GitHub last commit](https://img.shields.io/github/last-commit/pamidur/aspect-injector?style=flat-square)

[![Application Status](https://img.shields.io/github/workflow/status/pamidur/aspect-injector/Application?label=app%20build&style=flat-square)](https://github.com/pamidur/aspect-injector/commits/master)
[![Samples Status](https://img.shields.io/github/workflow/status/pamidur/aspect-injector/Samples?label=samples%20build&style=flat-square)](https://github.com/pamidur/aspect-injector/commits/master)

### Download
```bash
> dotnet add package AspectInjector
```

### Features
- Compile-time injection - works with Blazor and AOT
- Injecting **Before**, **After** and **Around** (wrap) **Methods**, **Constructors**, **Properties** and **Events**
- Injecting **Interface implementations**
- Supports any project that can reference **netstandard2.0** libraries, see [here](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
- Debugging support
- Roslyn analyzers for your convenience (only c# currently)
- Can work DI/IoC frameworks [#166](https://github.com/pamidur/aspect-injector/discussions/166)

Check out [samples](samples) and [docs](docs)

### Requirements
- (_semi-optional_) **Nuget 5.0+** for _transient build_ feature. All modern versions of VS and dotnetsdk have it. 
_(If you still use project.json for some reason - make sure you add AspectInjector to all projects in the solution)_
- (_optional_) For analyzers to work in VSCode, don't forget to enable ```"omnisharp.enableRoslynAnalyzers": true``` 

### Known Issues / Limitations
- Unsafe methods are not supported and are silently ignored.

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
