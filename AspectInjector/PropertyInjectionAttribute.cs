using System;

namespace AspectInjector
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class PropertyInjectionAttribute : Attribute
    {
        public PropertyInjectionAttribute(PropertyMethod method, MethodPoint point)
        {
            Method = method;
            Point = point;
        }

        public PropertyMethod Method { get; set; }
        public MethodPoint Point { get; set; }
    }
}