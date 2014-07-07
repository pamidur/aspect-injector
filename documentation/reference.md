    AspectInjector reference
========================

### Concept ###

Aspect is a class which contains a set of advices - methods which should be injected to certain points in the code. Each advice has mandatory attributes which define a kind of target class members (constructor, getter, setter, regular method etc.) and join points - points in the code where this advice should be injected (before target member, after or both). Aspects and advices are marked with appropriate attributes. For example, we have a class with one method marked as advice:

    class TraceAspect
	{
		private int count;

		[Advice(InjectionPoints.Before, InjectionTargets.Method)]
		public void CallCountTrace()
		{
			Console.WriteLine("Call #{0}", count);
			count++;
		}
	} 

Having it we can apply this aspect to any method or a set of methods of some class:

	//Method CallCountTrace of TraceAspect instance will be called at the beginning of Calculate() 
	[Aspect(typeof(TraceAspect))]
	public void Calculate() { }

	//Method CallCountTrace of TraceAspect instance will be called at the beginning of Load() and Save()
	[Aspect(typeof(TraceAspect))]
	class Container
	{
		public string Name { get; set; }

		public void Load() { }
		public void Save() { }
	}

	//Will not work - CallCountTrace() advice is applicable to regular methods only
	[Aspect(typeof(TraceAspect))]
	public string Name { get; set; }

Please note that there will be only one instace of an aspect per target class regardless of number of affected members. So in the example above Container class will have only one instance of TraceAspect, so both Load() and Save() will increment the same call counter.


### Attributes ###

**AspectAttribute**

Indicates that the aspect of the specified type should be applied to a specific class member or every class member matching the specified filter.

Parameters

|Name |Type |Description  |
|-----|-----|-------------|
|Type |Type |Specifies the class of the aspect which should be applied to the target member | 
|CustomData |object[] |Any custom data which then can be used by the aspect's implementation
|NameFilter |string | Specify a string which 
|AsseccModifierFilter| AccessModifiers |

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = true)]
    public sealed class AspectAttribute : Attribute

**AspectFactoryAttribute**

	[AttributeUsage(AttributeTargets.Method)]
    public sealed class AspectFactoryAttribute : Attribute

**AdviceAttribute**

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class AdviceAttribute : Attribute

**AdviceArgumentAttribute**

	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class AdviceArgumentAttribute : Attribute

**AdviceInterfaceProxyAttribute**

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AdviceInterfaceProxyAttribute : Attribute

### Enumerations ###
<br/>
**AccessModifiers**

Is used to specify target members filtering criteria for AspectAttribute, the meaning of the values correspond to the member access modifiers in C#.     

|Name |
|:----|
|Private|
|Protected|
|Internal|
|ProtectedInternal|
|Public|

**AdviceArgumentSource**

Is used to specify the source from which specific advice parameter should be populated. Corresponding required parameter types are shown in the table below.

|Name|Parameter type|Description|
|:---|:-------------|:----------|
|Instance| object | Target class instance.
|TargetName| string | The name of the target member
|TargetArguments| object[] | The array of target member arguments.
|TargetReturnValue| object | The return value of the target member.
|TargetException| Exception | An exception occurred in the target.
|AbortFlag| ref bool | A flag through which an advice can abort execution of the current target. Is appliable only for "Before" injection point and non-constructor targets.
|CustomData| object |

**InjectionPoints**

Is used to specify the point in the target class member, where the current aspect should be injected.

|Name|Description
|:---|:----------
|Before| Before the target body is executed.
|After| Aflter the target body is executed.
|Exception| 


**InjectionTargets**

Is used to specify the kind of target class members to which the crrent aspect should be injected.

|Name|
|:---|
|Constructor|
|Method|
|Getter|
|Setter|
|EventAdd|
|EventRemove|
