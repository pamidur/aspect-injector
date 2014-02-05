using System;

namespace AspectInjector
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MethodInjectionAttribute : Attribute
    {
        public MethodInjectionAttribute(MethodPoint point)
        {
            Point = point;
        }

        public MethodPoint Point { get; private set; }
    }
}