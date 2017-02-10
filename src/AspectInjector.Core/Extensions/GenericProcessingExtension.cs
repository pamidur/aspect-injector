using Mono.Cecil;
using Mono.Cecil.Rocks;
using System.Linq;

namespace AspectInjector.Core.Extensions
{
    public static class GenericProcessingExtension
    {
        public static MethodReference MakeHostInstanceGeneric(
                                  this MethodReference self,
                                  TypeReference context)
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

        public static TypeReference ParametrizeGenericChild(this TypeReference type, TypeReference child)
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

        public static MethodReference ParametrizeGenericChild(this MethodReference method, MethodReference child)
        {
            if (!method.HasGenericParameters || child.IsGenericInstance)
            {
                return child;
            }
            else
            {
                if (!child.HasGenericParameters)
                    return child;

                var result = new GenericInstanceMethod(child);
                method.GenericParameters.Select(method.ResolveGenericType).ToList().ForEach(result.GenericArguments.Add);

                return result;
            }
        }

        public static TypeReference ResolveGenericType(this TypeReference member, TypeReference mappingType)
        {
            if (member.IsGenericInstance && mappingType.IsGenericParameter)
                return ((IGenericInstance)member).ResolveGenericType((GenericParameter)mappingType);
            return mappingType;
        }

        public static TypeReference ResolveGenericType(this MethodReference member, TypeReference mappingType)
        {
            if (mappingType.IsGenericParameter)
            {
                if (member.IsGenericInstance)
                    return ((IGenericInstance)member).ResolveGenericType((GenericParameter)mappingType);
                else if (member.DeclaringType.IsGenericInstance)
                    return ((IGenericInstance)member.DeclaringType).ResolveGenericType((GenericParameter)mappingType);
            }
            return mappingType;
        }

        public static TypeReference ResolveGenericType(this IGenericInstance member, GenericParameter param)
        {
            dynamic resolvedMember = ((dynamic)member).Resolve();

            var owner = (IGenericParameterProvider)resolvedMember;
            var parent = resolvedMember.DeclaringType as TypeReference;

            if (param.Owner == owner)
                return member.GenericArguments[param.Position];
            else if (parent != null && parent.IsGenericInstance && param.Owner == parent.Resolve())
                return ((IGenericInstance)parent).ResolveGenericType(param);
            else
                return param;
        }
    }
}