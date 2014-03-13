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

        public void TestMethodFiltered2()
        {
            TestMethodFiltered1();
        }

        public void TestMethodFiltered3(string a, int b)
        {
        }

        public event EventHandler<EventArgs> TestEvent = (s, e) => { };

        public int TestProperty { get; set; }
    }

    internal class TestMethodAspect
    {
        //Property
        [Advice(Points = InjectionPoints.Before, Targets = InjectionTargets.Setter)]
        public void BeforeSetter()
        {
            Console.WriteLine("BeforeSetter");
        }

        [Advice(Points = InjectionPoints.After, Targets = InjectionTargets.Setter)]
        public void AfterSetter()
        {
            Console.WriteLine("AfterSetter");
        }

        [Advice(Points = InjectionPoints.Before, Targets = InjectionTargets.Getter)]
        public void BeforeGetter()
        {
            Console.WriteLine("BeforeGetter");
        }

        [Advice(Points = InjectionPoints.After, Targets = InjectionTargets.Getter)]
        public void AfterGetter()
        {
            Console.WriteLine("AfterGetter");
        }

        //Event
        [Advice(Points = InjectionPoints.Before, Targets = InjectionTargets.EventAdd)]
        public void BeforeEventAdd()
        {
            Console.WriteLine("BeforeEventAdd");
        }

        [Advice(Points = InjectionPoints.After, Targets = InjectionTargets.EventAdd)]
        public void AfterEventAdd()
        {
            Console.WriteLine("AfterEventAdd");
        }

        [Advice(Points = InjectionPoints.Before, Targets = InjectionTargets.EventRemove)]
        public void BeforeEventRemove()
        {
            Console.WriteLine("BeforeEventRemove");
        }

        [Advice(Points = InjectionPoints.After, Targets = InjectionTargets.EventRemove)]
        public void AfterEventRemove()
        {
            Console.WriteLine("AfterEventRemove");
        }

        //Method
        [Advice(Points = InjectionPoints.Before, Targets = InjectionTargets.Method)]
        public void BeforeMethod()
        {
            Console.WriteLine("BeforeMethod");
        }

        [Advice(Points = InjectionPoints.After, Targets = InjectionTargets.Method)]
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

        [Advice(Points = InjectionPoints.After, Targets = InjectionTargets.Constructor)]
        public void AfterConstructor()
        {
            Console.WriteLine("AfterConstructor");
        }
    }

    internal class TestMethodFilteredAspect
    {
        [Advice(Points = InjectionPoints.Before, Targets = InjectionTargets.Method)]
        public string BeforeMethod([AdviceArgument(Source = AdviceArgumentSource.TargetName)] string methodName, [AdviceArgument(Source = AdviceArgumentSource.TargetArguments)] object[] args)
        {
            Console.WriteLine("BeforeMethod({0}) filtered", methodName);
            return "";
        }
    }
}