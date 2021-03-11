using System;
using System.Collections.Generic;
using System.Reflection;

namespace Aspects.Universal.Events
{
    public class AspectEventArgs : EventArgs
    {
        public object Instance { get; internal set; }
        public Type Type { get; internal set; }
        public MethodBase Method { get; internal set; }
        public string Name { get; internal set; }
        public IReadOnlyList<object> Args { get; internal set; }
        public Type ReturnType { get; internal set; }
    }
}