using AspectInjector.Broker;
using System;
using System.Text;

namespace AspectInjector.Test
{
    internal class LogMethodCallAspect
    {
        [Advice(InjectionPoints.After, InjectionTargets.Method)]
        public void AfterMethod()
        {
            Console.WriteLine("Method executed");
        }

        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public void BeforeMethod()
        {
            Console.WriteLine("Method executing");
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

    internal class TestMethodAspect
    {
        [Advice(InjectionPoints.After, InjectionTargets.Constructor)]
        public void AfterConstructor()
        {
            Console.WriteLine("AfterConstructor");
        }

        [Advice(InjectionPoints.After, InjectionTargets.EventAdd)]
        public void AfterEventAdd()
        {
            Console.WriteLine("AfterEventAdd");
        }

        [Advice(InjectionPoints.After, InjectionTargets.EventRemove)]
        public void AfterEventRemove()
        {
            Console.WriteLine("AfterEventRemove");
        }

        [Advice(InjectionPoints.After, InjectionTargets.Getter)]
        public void AfterGetter()
        {
            Console.WriteLine("AfterGetter");
        }

        [Advice(InjectionPoints.After, InjectionTargets.Method)]
        public void AfterMethod()
        {
            Console.WriteLine("AfterMethod");
        }

        [Advice(InjectionPoints.After, InjectionTargets.Setter)]
        public void AfterSetter()
        {
            Console.WriteLine("AfterSetter");
        }

        //Constructor
        //[Advice(Points = InjectionPoints.Before, Targets = InjectionTargets.Constructor)]   <--there is a bug
        public void BeforeConstructor()
        {
            Console.WriteLine("BeforeConstructor");
        }

        //Event
        [Advice(InjectionPoints.Before, InjectionTargets.EventAdd)]
        public void BeforeEventAdd()
        {
            Console.WriteLine("BeforeEventAdd");
        }

        [Advice(InjectionPoints.Before, InjectionTargets.EventRemove)]
        public void BeforeEventRemove()
        {
            Console.WriteLine("BeforeEventRemove");
        }

        [Advice(InjectionPoints.Before, InjectionTargets.Getter)]
        public void BeforeGetter()
        {
            Console.WriteLine("BeforeGetter");
        }

        //Method
        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public void BeforeMethod()
        {
            Console.WriteLine("BeforeMethod");
        }

        //Property
        [Advice(InjectionPoints.Before, InjectionTargets.Setter)]
        public void BeforeSetter()
        {
            Console.WriteLine("BeforeSetter");
        }
    }

    // [Aspect(typeof(TestMethodFilteredAspect), NameFilter = "AAA", AccessModifierFilter = AccessModifiers.Public)]
    //[Aspect(typeof(TestMethodFilteredAspect2), NameFilter = "AAA", AccessModifierFilter = AccessModifiers.Public, CustomData = new object[] { "ololo", typeof(StringBuilder), 34, new object[] { 12, 31 } })]
    internal class TestMethodClass
    {
        private static readonly StringBuilder c = new StringBuilder();
        private readonly StringBuilder b = new StringBuilder();

        public event EventHandler<EventArgs> TestEvent = (s, e) => { };

        public TestMethodClass(string a)
        {
            b.Append(1);
            c.Append(1);
        }

        public TestMethodClass(int b)
        {
        }

        public int TestProperty { get; set; }

        public int TestMethodAAA(string data)
        {
            throw new NotImplementedException();
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

        private void TestMethodFiltered1()
        {
        }
    }

    internal class TestMethodFilteredAspect
    {
        [Advice(InjectionPoints.Exception | InjectionPoints.After | InjectionPoints.Before, InjectionTargets.Method)]
        public void InjectionMethod(
            [AdviceArgument(AdviceArgumentSource.TargetException)] Exception ex,
            [AdviceArgument(AdviceArgumentSource.TargetReturnValue)] object result,
            [AdviceArgument(AdviceArgumentSource.TargetName)] string targetName,
            [AdviceArgument(AdviceArgumentSource.TargetArguments)] object[] args,
            [AdviceArgument(AdviceArgumentSource.Instance)] object target,
            [AdviceArgument(AdviceArgumentSource.RoutableData)] object data

            //,            [AdviceArgument(AdviceArgumentSource.AbortFlag)] ref bool abort
            )
        {
        }
    }

    internal class TestMethodFilteredAspect2
    {
        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public int BeforeMethod(
            [AdviceArgument(AdviceArgumentSource.AbortFlag)] ref bool abort
            // , [AdviceArgument(AdviceArgumentSource.CustomData)] object[] cdata

             )
        {
            return 1;
        }
    }

    internal class ConstructorBeforeAspect
    {
        [Advice(InjectionPoints.Before, InjectionTargets.Constructor)]
        public void BeforeMethod()
        {
        }
    }

    internal class TestMethodFilteredAspect3
    {
        [Advice(InjectionPoints.Exception, InjectionTargets.Method)]
        public void ExcpMethod(
            [AdviceArgument(AdviceArgumentSource.TargetException)] Exception ex

              )
        {
        }
    }
}