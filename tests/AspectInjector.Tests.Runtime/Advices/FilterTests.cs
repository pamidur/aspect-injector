using AspectInjector.Broker;
using System;
using System.Runtime.InteropServices;
using Xunit;

namespace AspectInjector.Tests.Advices
{
    [SkipUnmanagedAndAbstractTests_Aspect]
    public abstract class SkipAbstractTests
    {
        public abstract int MessageBox();
    }

    [SkipUnmanagedAndAbstractTests_Aspect]
    public class SkipUnmanagedTests
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int MessageBox(IntPtr hWnd, String text, String caption, int options);
    }

    [Aspect(Scope.Global)]
    [Injection(typeof(SkipUnmanagedAndAbstractTests_Aspect))]
    public class SkipUnmanagedAndAbstractTests_Aspect :Attribute
    {
        [Advice(Kind.Around)]
        public object Trace()
        {
            return 0;
        }
    }

    public class FilterTests
    {
        [Fact]
        public void Advices_InjectAfterMethod_NameFilter()
        {
            Checker.Passed = false;

            var a = new FilterTests_Target();
            a.Do123();

            Assert.True(Checker.Passed);
        }

        [FilterTests_Aspect]
        public class FilterTests_Target
        {
            [FilterTests_Aspect]
            public void Do123()
            {
            }
        }

        [Aspect(Scope.Global)]
        [Injection(typeof(FilterTests_Aspect))]
        public class FilterTests_Aspect : Attribute
        {
            public int Counter = 0;

            [Advice(Kind.After, Targets = Target.Method)]
            public void AfterMethod()
            {
                Counter++;
                Checker.Passed = Counter == 1;
            }
        }
    }
}