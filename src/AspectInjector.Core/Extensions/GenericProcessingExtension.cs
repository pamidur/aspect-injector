using Mono.Cecil;
using Mono.Cecil.Rocks;
using System.Linq;

namespace AspectInjector.Core.Extensions
{
    public static class GenericProcessingExtension
    {
        public static MethodReference MakeHostInstanceGeneric(
                                  this MethodReference self,
                                  IGenericInstance context)
        {
            var reference = new MethodReference(
                self.Name,
                self.ReturnType,
                context.ParametrizeGenericChild(self.DeclaringType))
            {
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention
            };

            foreach (var parameter in self.Parameters)
            {
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
            }

            foreach (var genericParam in self.GenericParameters)
            {
                reference.GenericParameters.Add(new GenericParameter(genericParam.Name, self.Resolve()));
            }

            return reference;
        }

        public static TypeReference ParametrizeGenericChild(this MethodReference method, TypeReference child)
        {
            if (method.IsGenericInstance)
                return ((IGenericInstance)method).ParametrizeGenericChild(child);

            if (method.DeclaringType.IsGenericInstance)
                return ((IGenericInstance)method.DeclaringType).ParametrizeGenericChild(child);

            return child;
        }

        public static MethodReference ParametrizeGenericChild(this MethodReference method, MethodReference child)
        {
            if (method.IsGenericInstance)
                return ((IGenericInstance)method).ParametrizeGenericChild(child);

            if (method.DeclaringType.IsGenericInstance)
                return ((IGenericInstance)method.DeclaringType).ParametrizeGenericChild(child);

            if (method.HasGenericParameters)
                return ((IGenericParameterProvider)method).ParametrizeGenericChild(child);

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

        public static MethodReference ParametrizeGenericChild(this TypeReference type, MethodReference child)
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

        public static MethodReference ParametrizeGenericChild(this IGenericInstance type, MethodReference child)
        {
            if (child.IsGenericInstance)
            {
                var nestedGeneric = (GenericInstanceMethod)child;

                if (!nestedGeneric.ContainsGenericParameter)
                    return nestedGeneric;

                var args = nestedGeneric.GenericArguments.Select(ga => type.ResolveGenericType(ga)).ToArray();

                var result = new GenericInstanceMethod(child.MakeHostInstanceGeneric(type));
                args.ToList().ForEach(result.GenericArguments.Add);

                return result;
            }
            else
            {
                if (!child.HasGenericParameters)
                    return child;

                var result = new GenericInstanceMethod(child.MakeHostInstanceGeneric(type));
                child.GenericParameters.Select(type.ResolveGenericType).ToList().ForEach(result.GenericArguments.Add);

                return result;
            }
        }

        public static MethodReference ParametrizeGenericChild(this IGenericParameterProvider type, MethodReference child)
        {
            if (child.IsGenericInstance)
            {
                return child;
            }
            else
            {
                if (!child.HasGenericParameters)
                    return child;

                var result = new GenericInstanceMethod(child);
                type.GenericParameters.ToList().ForEach(result.GenericArguments.Add);

                return result;
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