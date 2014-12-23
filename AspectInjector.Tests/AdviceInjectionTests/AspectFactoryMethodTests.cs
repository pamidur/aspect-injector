using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests.AspectFactoryMethod
{
	[TestClass]
	public class AspectFactoryMethodTests
	{
		[TestMethod]
		public void Create_Aspect_Through_Factory_Method()
		{
			Checker.Passed = false;
			var test = new TestClass();
			Assert.IsTrue(Checker.Passed);
		}
	}

	[Aspect(typeof(TestAspect))]
	public class TestClass
	{
	}

	public class TestAspect
	{
		[Advice(InjectionPoints.After, InjectionTargets.Constructor)]
		public void TestAdvice()
		{
		}

		[AspectFactory]
		public static TestAspect Create()
		{
			Checker.Passed = true;
			return new TestAspect();
		}
	}
}