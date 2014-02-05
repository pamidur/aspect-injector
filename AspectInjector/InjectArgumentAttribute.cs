using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector
{
    public enum InjectArgument
    {
        Instanse,
        MemberName,
        MemberArguments
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class InjectArgumentAttribute : Attribute
    {
        public InjectArgument Argument { get; set; }
    }
}