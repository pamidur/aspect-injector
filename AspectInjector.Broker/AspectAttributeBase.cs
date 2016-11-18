using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AspectInjector.Broker
{
    public abstract class AspectAttributeBase : Attribute
    {
        internal AspectAttributeBase()
        {
        }

        public Type Type { get; protected set; }

        public string NameFilter { get; set; }

        public AccessModifiers AccessModifierFilter { get; set; }
    }
}