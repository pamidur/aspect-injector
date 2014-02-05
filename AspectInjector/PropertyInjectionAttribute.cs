using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector
{
    public enum PropertyMethod
    {
        Get, Set
    }

    public enum MethodPoint
    {
        Begining, Ending
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class PropertyInjectionAttribute : Attribute
    {
        public PropertyMethod Method { get; set; }

        public MethodPoint Point { get; set; }
    }
}