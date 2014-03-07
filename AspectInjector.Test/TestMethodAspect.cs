using AspectInjector.Broker;
using System;
using System.Text;

namespace AspectInjector.Test
{
    [Aspect(typeof(TestMethodAspect))]
    internal class TestMethodClass
    {
        private readonly StringBuilder b = new StringBuilder();

        public TestMethodClass(string a)
        {
        }

        public TestMethodClass(int b)
        {
        }

        public void TestMethod(string data)
        {
        }

        public event EventHandler<EventArgs> TestEvent = (s, e) => { };

        public int TestProperty { get; set; }
    }

    internal class TestMethodAspect
    {
        //Property
        [Advice(Points = InjectionPoint.Before, Targets = InjectionTarget.Setter)]
        public void BeforeSetter()
        {
            Console.WriteLine("BeforeSetter");
        }

        [Advice(Points = InjectionPoint.After, Targets = InjectionTarget.Setter)]
        public void AfterSetter()
        {
            Console.WriteLine("AfterSetter");
        }

        [Advice(Points = InjectionPoint.Before, Targets = InjectionTarget.Getter)]
        public void BeforeGetter()
        {
            Console.WriteLine("BeforeGetter");
        }

        [Advice(Points = InjectionPoint.After, Targets = InjectionTarget.Getter)]
        public void AfterGetter()
        {
            Console.WriteLine("AfterGetter");
        }

        //Event
        [Advice(Points = InjectionPoint.Before, Targets = InjectionTarget.EventAdd)]
        public void BeforeEventAdd()
        {
            Console.WriteLine("BeforeEventAdd");
        }

        [Advice(Points = InjectionPoint.After, Targets = InjectionTarget.EventAdd)]
        public void AfterEventAdd()
        {
            Console.WriteLine("AfterEventAdd");
        }

        [Advice(Points = InjectionPoint.Before, Targets = InjectionTarget.EventRemove)]
        public void BeforeEventRemove()
        {
            Console.WriteLine("BeforeEventRemove");
        }

        [Advice(Points = InjectionPoint.After, Targets = InjectionTarget.EventRemove)]
        public void AfterEventRemove()
        {
            Console.WriteLine("AfterEventRemove");
        }

        //Method
        [Advice(Points = InjectionPoint.Before, Targets = InjectionTarget.Method)]
        public void BeforeMethod()
        {
            Console.WriteLine("BeforeMethod");
        }

        [Advice(Points = InjectionPoint.After, Targets = InjectionTarget.Method)]
        public void AfterMethod()
        {
            Console.WriteLine("AfterMethod");
        }

        //Constructor
        //[Advice(Points = InjectionPoint.Before, Targets = InjectionTarget.Constructor)]   <--there is a bug
        public void BeforeConstructor()
        {
            Console.WriteLine("BeforeConstructor");
        }

        [Advice(Points = InjectionPoint.After, Targets = InjectionTarget.Constructor)]
        public void AfterConstructor()
        {
            Console.WriteLine("AfterConstructor");
        }
    }
}