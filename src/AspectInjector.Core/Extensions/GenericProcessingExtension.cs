using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
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

                var args = nestedGeneric.GenericArguments.Select(ga => member.ResolveIfGeneric(ga)).ToArray();

                return child.Resolve().MakeGenericInstanceType(args);
            }
            else
            {
                if (!child.HasGenericParameters)
                    return child;

                return child.MakeGenericInstanceType(child.GenericParameters.Select(member.ResolveGenericType).ToArray());
            }
        }

        public static MethodReference ParametrizeGenericChild(this MemberReference member, MethodReference child)
        {
            var paramProvider = member as IGenericParameterProvider;
            if (paramProvider == null)
                return child;

            if (child.IsGenericInstance)
            {
                return child;
            }
            else
            {
                if (!child.HasGenericParameters)
                    return child;

                var result = new GenericInstanceMethod(child);
                paramProvider.GenericParameters.Select(member.ResolveGenericType).ToList().ForEach(result.GenericArguments.Add);

                return result;
            }
        }

        public static TypeReference ResolveIfGeneric(this MemberReference member, TypeReference param)
        {
            if (param.ContainsGenericParameter)
                return member.ResolveGenericType(param);

            return param;
        }

        public static TypeReference ResolveGenericType(this MemberReference member, TypeReference param)
        {
            if (!param.ContainsGenericParameter)
                throw new Exception($"{param} is not generic!");

            if (param.IsByReference && param.ContainsGenericParameter)
                return new ByReferenceType(member.ResolveGenericType(param.GetElementType()));

            if (param.IsGenericInstance)
            {
                var nestedGeneric = (GenericInstanceType)param;
                var args = nestedGeneric.GenericArguments.Select(ga => member.ResolveIfGeneric(ga)).ToArray();
                return param.Module.Import(param.Resolve()).MakeGenericInstanceType(args);
            }

            var gparam = param as GenericParameter;
            if (gparam == null)
                throw new Exception("Cannot resolve generic parameter");

            object resolvedMember = ((dynamic)member).Resolve();
            object resolvedOwner = ((dynamic)gparam.Owner).Resolve();

            if (resolvedOwner == resolvedMember)
            {
                if (member is IGenericInstance)
                    return (member as IGenericInstance).GenericArguments[gparam.Position];
                else
                    return ((IGenericParameterProvider)member).GenericParameters[gparam.Position];
            }
            else if (member.DeclaringType != null)
                return member.DeclaringType.ResolveGenericType(gparam);
            else
                throw new Exception("Cannot resolve generic parameter");
        }
    }
}