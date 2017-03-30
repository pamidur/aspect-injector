using AspectInjector.Broker;
using System;
using System.Runtime.InteropServices;

namespace AspectInjector.Tests.General
{
    //The compilation is just failed if injector tries to process external methods
    [Inject(typeof(UnmanagedTests_Aspect))]
    public class UnmanagedTests
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int MessageBox(IntPtr hWnd, String text, String caption, int options);
    }

    [Aspect(Aspect.Scope.Global)]
    internal class UnmanagedTests_Aspect
    {
        [Advice(Advice.Type.After, Advice.Target.Method)]
        public void Trace()
        {
        }
    }
}