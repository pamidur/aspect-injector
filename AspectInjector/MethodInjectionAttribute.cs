using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MethodInjectionAttribute : Attribute
    {
        public MethodPoint Point { get; set; }
    }
}