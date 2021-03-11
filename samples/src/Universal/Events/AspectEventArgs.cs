using System;
using System.Collections.Generic;
using System.Reflection;

namespace Aspects.Universal.Events
{
    public class AspectEventArgs : EventArgs
    {
        public object Instance { get; set; }
        public Type Type { get; set; }
        public MethodBase Method { get; set; }
        public string Name { get; set; }
        public IReadOnlyList<object> Args { get; set; }
        public Type ReturnType { get; set; }
    }
}