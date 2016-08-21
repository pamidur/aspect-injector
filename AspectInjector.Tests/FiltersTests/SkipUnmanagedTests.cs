using AspectInjector.Broker;
using System;
using System.Runtime.InteropServices;

namespace AspectInjector.Tests.FiltersTests
{
    //The compilation is just failed if injector tries to process external methods
    [Aspect(typeof(SkipUnmanagedTests_Aspect))]
    public class SkipUnmanagedTests
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int MessageBox(IntPtr hWnd, String text, String caption, int options);
    }

    internal class SkipUnmanagedTests_Aspect
    {
        [Advice(InjectionPoints.After, InjectionTargets.Method)]
        public void Trace()
        {
        }
    }
}
