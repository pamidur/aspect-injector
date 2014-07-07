AspectInjector
==========

Description here

## Motivation

Why we do what we do

## Features

* Injection aspects into **methods**.
* Injection aspects into **properties**.
* Injection aspects into **events**.
* Injection aspects into **constructors**.
* Injection of **interface proxies**.
* **No external dependencies**. Reference to `AspectInjector.Broker.dll` will be deleted at compile-time.
* Possibility to **abort method** and **replace return value**

## Code Pitch

### The very first sample.

#### Implementing simple aspect.

Let's implement a simple aspect that logs method calls into console.

	class LogMethodCallAspect
	{
		[Advice(InjectionPoints.Before, InjectionTargets.Method)]
		public void BeforeMethod()
		{
			Console.WriteLine("Method executing");
		}

		[Advice(InjectionPoints.After, InjectionTargets.Method)]
		public void AfterMethod()
		{
			Console.WriteLine("Method executed");
		}
	}

#### Injeting aspect into class.

Marking method with `Aspect` attribute.

	class MyClass
	{
		[Aspect(typeof(LogMethodCallAspect))]
		public void Do()
		{
			Console.WriteLine("Here I am!");
		}
	}

#### Getting result.

Let's compile and decompile code to see what happened

	class MyClass
	{
		private readonly LogMethodCallAspect __a_LogMethodCallAspect = new LogMethodCallAspect();
		public void Do()
		{
			this.__a_LogMethodCallAspect.BeforeMethod();
			Console.WriteLine("Here I am!");
			this.__a_LogMethodCallAspect.AfterMethod();
		}
	}

Note that there are no more attribures connected to AspectInjector. They were deleted. So were attributes from aspect itself:

	class LogMethodCallAspect
	{
		public void BeforeMethod()
		{
			Console.WriteLine("Method executing");
		}
		public void AfterMethod()
		{
			Console.WriteLine("Method executed");
		}
	}

## Credits

Daveloped by **Yuriy Ivon, Alexander Guly**.