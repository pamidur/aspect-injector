using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests
{
	[TestClass]
	public class AspectFactoryMethodTests
	{
		[TestMethod]
		public void Create_Aspect_Through_Factory_Method()
		{
			Checker.Passed = false;
			var test = new AspectFactoryMethodTests_Target();
			Assert.IsTrue(Checker.Passed);
		}
	}

	[Aspect(typeof(AspectFactoryMethodTests_Aspect))]
	public class AspectFactoryMethodTests_Target
	{
	}

	public class AspectFactoryMethodTests_Aspect
	{
		[Advice(InjectionPoints.After, InjectionTargets.Constructor)]
		public void TestMethod()
		{
		}

		[AspectFactory]
		public static AspectFactoryMethodTests_Aspect Create()
		{
			Checker.Passed = true;
			return new AspectFactoryMethodTests_Aspect();
		}
	}
}