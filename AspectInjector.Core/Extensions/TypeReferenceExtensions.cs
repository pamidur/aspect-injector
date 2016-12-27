using AspectInjector.Core.Models;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Extensions
{
    public static class TypeReferenceExtensions
    {
        public static bool IsTypeOf(this TypeReference tr1, TypeReference tr2)
        {
            return FQN.FromTypeReference(tr1).Equals(FQN.FromTypeReference(tr2));
        }

        public static bool IsTypeOf(this TypeReference tr, Type type)
        {
            return FQN.FromTypeReference(tr).Equals(FQN.FromType(type));
        }

        public static FQN GetFQN(this TypeReference type)
        {
            return FQN.FromTypeReference(type);
        }

        public static FQN GetFQN(this Type type)
        {
            return FQN.FromType(type);
        }

        public static IEnumerable<TypeDefinition> GetTypesTree(this TypeDefinition type)
        {
            yield return type;

            foreach (var nestedType in type.NestedTypes

                .SelectMany(t => GetTypesTree(t)))
            {
                yield return nestedType;
            }
        }

        public static bool Implements(this TypeReference tr, TypeReference @interface)
        {
            var td = tr.Resolve();
            var ti = @interface;

            return td.Interfaces.Any(i => i.IsTypeOf(ti)) || (td.BaseType != null && td.BaseType.Implements(ti));
        }

        //public static IEnumerable<MethodReference> GetMethodRefs(this TypeReference tr, ExtendedTypeSystem ts)
        //{
        //    var def = tr.Resolve();
        //    var methods = def.Methods.Select(d => d.CreateReference(ts));

        //    GenericParameter a;
        //    a.
        //}
    }
}