using AspectInjector.Core.Models;
using Mono.Cecil;
using System;

namespace AspectInjector.Core.Extensions
{
    public static class TypeReferenceExtensions
    {
        public static bool IsTypeOf(this TypeReference tr1, TypeReference tr2)
        {
            return FQN.FromTypeReference(tr1) == FQN.FromTypeReference(tr2);
        }

        public static bool IsTypeOf(this TypeReference tr, Type type)
        {
            return FQN.FromTypeReference(tr) == FQN.FromType(type);
        }
    }
}