using AspectInjector.Broker;
using System;
using System.Text;

namespace AspectInjector.Test
{
    [Aspect(typeof(TestMethodAspect))]
    [Aspect(typeof(TestMethodFilteredAspect), NameFilter = "Filtered", AccessModifierFilter = AccessModifiers.Public)]
    internal class TestMethodClass
    {
        private readonly StringBuilder b = new StringBuilder();
        private static readonly StringBuilder c = new StringBuilder();

        public TestMethodClass(string a)
        {
            b.Append(1);
            c.Append(1);
        }

        public TestMethodClass(int b)
        {
        }

        public void TestMethod(string data)
        {
        }

        private void TestMethodFiltered1()
        {
        }

        public string TestMethodFiltered2()
        {
            TestMethodFiltered1();
            return "aaa";
        }

        public string TestMethodFiltered3(string a, int b)
        {
            return "sdfsdfs";
        }

        public event EventHandler<EventArgs> TestEvent = (s, e) => { };

        public int TestProperty { get; set; }
    }

    internal class TestMethodFilteredAspect
    {
        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public string BeforeMethod(
            [AdviceArgument(AdviceArgumentSource.TargetName)] string methodName,
            [AdviceArgument(AdviceArgumentSource.TargetArguments)] object[] args,
            [AdviceArgument(AdviceArgumentSource.AbortFlag)] ref bool abort

            )
        {
            Console.WriteLine("BeforeMethod({0}) filtered", methodName);
            return "";
        }
    }

    internal class TestMethodAspect
    {
        //Property
        [Advice(InjectionPoints.Before, InjectionTargets.Setter)]
        public void BeforeSetter()
        {
            Console.WriteLine("BeforeSetter");
        }

        [Advice(InjectionPoints.After, InjectionTargets.Setter)]
        public void AfterSetter()
        {
            Console.WriteLine("AfterSetter");
        }

        [Advice(InjectionPoints.Before, InjectionTargets.Getter)]
        public void BeforeGetter()
        {
            Console.WriteLine("BeforeGetter");
        }

        [Advice(InjectionPoints.After, InjectionTargets.Getter)]
        public void AfterGetter()
        {
            Console.WriteLine("AfterGetter");
        }

        //Event
        [Advice(InjectionPoints.Before, InjectionTargets.EventAdd)]
        public void BeforeEventAdd()
        {
            Console.WriteLine("BeforeEventAdd");
        }

        [Advice(InjectionPoints.After, InjectionTargets.EventAdd)]
        public void AfterEventAdd()
        {
            Console.WriteLine("AfterEventAdd");
        }

        [Advice(InjectionPoints.Before, InjectionTargets.EventRemove)]
        public void BeforeEventRemove()
        {
            Console.WriteLine("BeforeEventRemove");
        }

        [Advice(InjectionPoints.After, InjectionTargets.EventRemove)]
        public void AfterEventRemove()
        {
            Console.WriteLine("AfterEventRemove");
        }

        //Method
        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public void BeforeMethod()
        {
            Console.WriteLine("BeforeMethod");
        }

        [Advice(InjectionPoints.After, InjectionTargets.Method)]
        public void AfterMethod()
        {
            Console.WriteLine("AfterMethod");
        }

        //Constructor
        //[Advice(Points = InjectionPoints.Before, Targets = InjectionTargets.Constructor)]   <--there is a bug
        public void BeforeConstructor()
        {
            Console.WriteLine("BeforeConstructor");
        }

        [Advice(InjectionPoints.After, InjectionTargets.Constructor)]
        public void AfterConstructor()
        {
            Console.WriteLine("AfterConstructor");
        }
    }
}