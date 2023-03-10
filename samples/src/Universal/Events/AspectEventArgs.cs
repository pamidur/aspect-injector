using System;
using System.Collections.Generic;
using System.Reflection;

namespace Aspects.Universal.Events
{
    public class AspectEventArgs : EventArgs
    {
        public object? Instance { get; internal set; }
        public Type Type { get; internal set; } = null!;
        public MethodBase Method { get; internal set; } = null!;
        public string Name { get; internal set; } = null!;
        public IReadOnlyList<object> Args { get; internal set; } = null!;
        public Type ReturnType { get; internal set; } = null!;
        public Attribute[] Triggers { get; internal set; } = null!;
    }
}
