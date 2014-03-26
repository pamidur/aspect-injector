using AspectInjector.Broker;
using System;
using System.Text;

namespace AspectInjector.Test
{
    //[Aspect(typeof(TestMethodFilteredAspect), NameFilter = "AAA", AccessModifierFilter = AccessModifiers.Public)]
    [Aspect(typeof(TestMethodFilteredAspect2), NameFilter = "AAA", AccessModifierFilter = AccessModifiers.Public)]
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

        public int TestMethodAAA(string data)
        {
            throw new NotImplementedException();
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
        public int BeforeMethod(
            [AdviceArgument(AdviceArgumentSource.AbortFlag)] ref bool abort

            )
        {
            return 1;
        }
    }

    internal class TestMethodFilteredAspect2
    {
        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public int BeforeMethod(
            [AdviceArgument(AdviceArgumentSource.AbortFlag)] ref bool abort
            )
        {
            return 2;
        }
    }

    internal class MyClass
    {
        [Aspect(typeof(LogMethodCallAspect))]
        public void Do()
        {
            Console.WriteLine("Here I am!");
        }
    }

    internal class LogMethodCallAspect
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