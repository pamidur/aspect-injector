using AspectInjector.Core.Fluent.Models;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Core.Extensions
{
    public static class MemberReferenceExtensions
    {
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
                var methodReferenceCopy = new MethodReference(member.Name, (TypeReference)CreateReference(methodReference.ReturnType, ts), declaringType)
                {
                    HasThis = methodReference.HasThis,
                    ExplicitThis = methodReference.ExplicitThis,
                    CallingConvention = methodReference.CallingConvention
                };

                foreach (var parameter in methodReference.Parameters)
                {
                    methodReferenceCopy.Parameters.Add(new ParameterDefinition((TypeReference)CreateReference(parameter.ParameterType, ts)));
                }

                return methodReferenceCopy;
            }

            throw new NotSupportedException("Not supported member type " + member.GetType().FullName);
        }
    }
}