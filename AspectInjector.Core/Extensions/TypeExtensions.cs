using AspectInjector.Core.Models;
using System;

namespace AspectInjector.Core.Extensions
{
    public static class TypeExtensions
    {
        public static FQN GetFQN(this Type type)
        {
            return FQN.FromType(type);
        }
    }
}