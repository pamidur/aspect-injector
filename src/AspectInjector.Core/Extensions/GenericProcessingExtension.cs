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

        public static FieldReference ParametrizeGenericChild(this MemberReference member, FieldReference child)
        {
            return new FieldReference(child.Name, child.FieldType, member.ParametrizeGenericChild(child.DeclaringType));
        }

        public static TypeReference ParametrizeGenericChild(this MemberReference member, TypeReference child)
        {
            if (child.IsGenericInstance)
            {
                var nestedGeneric = (GenericInstanceType)child;

                if (!nestedGeneric.ContainsGenericParameter)
                    return nestedGeneric;

                var args = nestedGeneric.GenericArguments.Select(ga => member.ResolveGenericType(ga)).ToArray();

                return child.Resolve().MakeGenericInstanceType(args);
            }
            else
            {
                if (!child.HasGenericParameters)
                    return child;

                return child.MakeGenericInstanceType(child.GenericParameters.Select(member.ResolveGenericType).ToArray());
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

        public static TypeReference ResolveGenericType(this MemberReference member, TypeReference param)
        {
            var gparam = param as GenericParameter;
            if (gparam == null)
                return param;

            var generic = member as IGenericInstance;
            if (generic == null)
                return member.DeclaringType == null ? param : member.DeclaringType.ResolveGenericType(param);

            dynamic resolvedMember = ((dynamic)member).Resolve();

            var owner = (IGenericParameterProvider)resolvedMember;
            var parent = resolvedMember.DeclaringType as TypeReference;

            if (gparam.Owner == owner)
                return generic.GenericArguments[gparam.Position];
            else if (parent != null && parent.IsGenericInstance && gparam.Owner == parent.Resolve())
                return parent.ResolveGenericType(gparam);
            else
                return param;
        }
    }
}