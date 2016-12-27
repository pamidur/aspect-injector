using AspectInjector.Core.Fluent.Models;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
using System.Linq;

namespace AspectInjector.Core.Extensions
{
    public static class MemberReferenceExtensions
    {
        public static MethodReference MakeHostInstanceGeneric(
                                  this MethodReference self,
                                  params TypeReference[] args)
        {
            var reference = new MethodReference(
                self.Name,
                self.ReturnType,
                self.DeclaringType.MakeGenericInstanceType(args))
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
                //reference.GenericParameters.Add(new GenericParameter(genericParam.Name, reference));
                reference.GenericParameters.Add(genericParam);
            }

            return reference;
        }

        public static bool IsImplementationOf(this MethodDefinition m, MethodReference ifaceMethod)
        {
            if (m.IsExplicitImplementationOf(ifaceMethod))
                return true;

            var ifaceMethodDef = ifaceMethod.Resolve();

            if (m.Name == ifaceMethod.Name && m.Parameters.Count == ifaceMethodDef.Parameters.Count)
            {
                for (int i = 0; i < m.Parameters.Count; i++)
                    if (!m.Parameters[i].ParameterType.IsTypeOf(ifaceMethod.ResolveGenericType(ifaceMethodDef.Parameters[i].ParameterType)))
                        return false;

                return true;
            }

            return false;
        }

        public static bool IsExplicitImplementationOf(this MethodDefinition m, MethodReference ifaceMethod)
        {
            if (m.Overrides.Any(o => o.FullName == ifaceMethod.FullName))
                return true;

            return false;
        }

        public static MemberReference CreateReference(this MemberReference member, ExtendedTypeSystem ts)
        {
            var module = ts.GetModule();

            if (member is TypeReference)
            {
                if (member is IGenericParameterProvider)
                {
                    ((IGenericParameterProvider)member).GenericParameters.ToList().ForEach(tr => CreateReference(tr, ts));
                }

                if (member.Module == module || ((TypeReference)member).IsGenericParameter)
                    return member;

                return ts.Import((TypeReference)member);
            }

            var declaringType = (TypeReference)CreateReference(member.DeclaringType, ts);
            var generic = member.DeclaringType as IGenericParameterProvider;

            if (generic != null && generic.HasGenericParameters)
            {
                declaringType = new GenericInstanceType((TypeReference)CreateReference(member.DeclaringType, ts));
                generic.GenericParameters.ToList()
                    .ForEach(tr => ((IGenericInstance)declaringType).GenericArguments.Add((TypeReference)CreateReference(tr, ts)));
            }

            var fieldReference = member as FieldReference;
            if (fieldReference != null)
                return new FieldReference(member.Name, (TypeReference)CreateReference(fieldReference.FieldType, ts), declaringType);

            var methodReference = member as MethodReference;
            if (methodReference != null)
            {
                //TODO: more fields may need to be copied
                var methodReferenceCopy = new MethodReference(member.Name, (TypeReference)CreateReference(methodReference.SafeReturnType(), ts), declaringType)
                {
                    HasThis = methodReference.HasThis,
                    ExplicitThis = methodReference.ExplicitThis,
                    CallingConvention = methodReference.CallingConvention
                };

                foreach (var parameter in methodReference.Parameters)
                {
                    methodReferenceCopy.Parameters.Add(new ParameterDefinition((TypeReference)CreateReference(methodReference.ResolveGenericType(parameter.ParameterType), ts)));
                }

                return methodReferenceCopy;
            }

            throw new NotSupportedException("Not supported member type " + member.GetType().FullName);
        }
    }
}