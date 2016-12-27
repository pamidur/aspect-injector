using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Core.Extensions
{
    public static class GenericProcessingExtension
    {
        public static TypeReference ParametrizeGenericChild(this MethodReference method, TypeReference child)
        {
            if (method.IsGenericInstance)
                return ((IGenericInstance)method).ParametrizeGenericChild(child);

            if (method.DeclaringType.IsGenericInstance)
                return ((IGenericInstance)method.DeclaringType).ParametrizeGenericChild(child);

            return child;
        }

        public static TypeReference ResolveGenericType(this MethodReference method, TypeReference mappingType)
        {
            var result = mappingType;

            if (mappingType.IsGenericParameter)
            {
                var gp = (GenericParameter)mappingType;

                if (gp.Owner == method.Resolve() && method.IsGenericInstance)
                    result = ((IGenericInstance)method).GenericArguments[gp.Position];
                else if (gp.Owner == method.DeclaringType.Resolve() && method.DeclaringType.IsGenericInstance)
                    result = ((IGenericInstance)method.DeclaringType).GenericArguments[gp.Position];
            }

            result = method.ParametrizeGenericChild(result);

            return result;
        }

        public static TypeReference ParametrizeGenericChild(this TypeReference type, TypeReference child)
        {
            if (type.IsGenericInstance)
                return ((IGenericInstance)type).ParametrizeGenericChild(child);

            return child;
        }

        public static TypeReference ParametrizeGenericChild(this IGenericInstance type, TypeReference child)
        {
            if (child.IsGenericInstance)
            {
                var nestedGeneric = (GenericInstanceType)child;

                if (!nestedGeneric.ContainsGenericParameter)
                    return nestedGeneric;

                var args = nestedGeneric.GenericArguments.Select(ga => type.ResolveGenericType(ga)).ToArray();

                return child.Resolve().MakeGenericInstanceType(args);
            }
            else
            {
                if (!child.HasGenericParameters)
                    return child;

                return child.MakeGenericInstanceType(child.GenericParameters.Select(type.ResolveGenericType).ToArray());
            }
        }

        public static TypeReference ResolveGenericType(this TypeReference type, TypeReference mappingType)
        {
            var result = mappingType;

            if (mappingType.IsGenericParameter)
            {
                var gp = (GenericParameter)mappingType;

                if (gp.Owner == type.Resolve() && type.IsGenericInstance)
                    result = ((IGenericInstance)type).GenericArguments[gp.Position];
            }

            result = type.ParametrizeGenericChild(result);

            return result;
        }

        public static TypeReference ResolveGenericType(this IGenericInstance member, TypeReference mappingType)
        {
            if (member is TypeReference)
                return ((TypeReference)member).ResolveGenericType(mappingType);

            if (member is MethodReference)
                return ((MethodReference)member).ResolveGenericType(mappingType);

            return mappingType;
        }

        public static TypeReference SafeReturnType(this MethodReference method)
        {
            return method.ResolveGenericType(method.ReturnType);
        }
    }
}