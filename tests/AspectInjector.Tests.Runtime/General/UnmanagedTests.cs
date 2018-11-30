using AspectInjector.Broker;
using System;
using System.Runtime.InteropServices;

namespace AspectInjector.Tests.General
{
    //The compilation is just failed if injector tries to process external methods
    [UnmanagedTests_Aspect]
    public class UnmanagedTests
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int MessageBox(IntPtr hWnd, String text, String caption, int options);
    }

    [Aspect(Aspect.Scope.Global)]
    [InjectionTrigger(typeof(UnmanagedTests_Aspect))]
    internal class UnmanagedTests_Aspect : Attribute
    {
        [Advice(Advice.Kind.After, Targets = Advice.Target.Method)]
        public void Trace()
        {
        }
    }
}