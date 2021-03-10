using System;
using System.Reflection;
using Aspects.Universal.Attributes;

namespace Aspects.Universal.Events
{
    public class AspectEventArgs : EventArgs
    {
        public object Instance { get; set; }
        public Type Type { get; set; }
        public MethodBase Method { get; set; }
        public string Name { get; set; }
        public object[] Args { get; set; }
        public Type ReturnType { get; set; }
        public BaseUniversalWrapperAttribute[] Triggers { get; set; }

        public AspectEventArgs Copy()
        {
            return (AspectEventArgs)MemberwiseClone();
        }
    }
}