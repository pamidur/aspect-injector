using System;

namespace AspectInjector
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class ArgumentInjectionAttribute : Attribute
    {
        public ArgumentInjectionAttribute(ArgumentValue value)
        {
            ArgumentValue = value;
        }

        public ArgumentValue ArgumentValue { get; private set; }
    }
}