using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AspectInjector.Broker
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AspectUsage : Attribute
    {
        public CreationScope Scope { get; set; }

        public enum CreationScope
        {
            Global,
            PerType,
            PerInstance
        }
    }
}