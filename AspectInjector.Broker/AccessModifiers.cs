using System;
using System.Collections.Generic;
using System.Text;

namespace AspectInjector.Broker
{
    [Flags]
    public enum AccessModifiers
    {
        Any = 0,
        Private = 1,
        Protected = 2,
        Internal = 4,
        ProtectedInternal = 8,
        Public = 16
    }
}