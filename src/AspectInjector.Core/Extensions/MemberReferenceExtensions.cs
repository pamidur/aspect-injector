using AspectInjector.Core.Fluent;
using Mono.Cecil;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AspectInjector.Core.Extensions
{
    public static class MemberReferenceExtensions
    {
        public static bool IsAsync(this MethodDefinition m)
        {
            return m.CustomAttributes.Any(a => a.AttributeType.IsTypeOf(typeof(AsyncStateMachineAttribute)));
        }

        public static bool IsIterator(this MethodDefinition m)
        {
            return m.CustomAttributes.Any(a => a.AttributeType.IsTypeOf(typeof(IteratorStateMachineAttribute)));
        }

        public static bool IsNormalMethod(this MethodDefinition m)
        {
            return !m.IsAddOn && !m.IsRemoveOn && !m.IsSetter && !m.IsGetter && !m.IsConstructor;
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

        //public static MethodDefinition CopyDefinition(this MethodDefinition origin, ModuleDefinition module)
        //{
        //    var ts = module.GetTypeSystem();

        //    var method = new MethodDefinition(origin.Name, origin.Attributes, ts.Import(origin.ReturnType));

        //    foreach (var gparam in origin.GenericParameters)
        //        method.GenericParameters.Add(new GenericParameter(gparam.Name, method));

        //    if (method.ReturnType.IsGenericParameter)
        //        method.ReturnType = method.GenericParameters[origin.GenericParameters.IndexOf((GenericParameter)method.ReturnType)];
        //}

        //public static MemberReference CreateReference(this MemberReference member, ExtendedTypeSystem ts)
        //{
        //    var module = ts.GetModule();

        //    if (member is TypeReference)
        //    {
        //        if (member is IGenericParameterProvider)
        //        {
        //            ((IGenericParameterProvider)member).GenericParameters.ToList().ForEach(tr => CreateReference(tr, ts));
        //        }

        //        if (member.Module == module || ((TypeReference)member).IsGenericParameter)
        //            return member;

        //        return ts.Import((TypeReference)member);
        //    }

        //    var declaringType = (TypeReference)CreateReference(member.DeclaringType, ts);
        //    var generic = member.DeclaringType as IGenericParameterProvider;

        //    if (generic != null && generic.HasGenericParameters)
        //    {
        //        declaringType = new GenericInstanceType((TypeReference)CreateReference(member.DeclaringType, ts));
        //        generic.GenericParameters.ToList()
        //            .ForEach(tr => ((IGenericInstance)declaringType).GenericArguments.Add((TypeReference)CreateReference(tr, ts)));
        //    }

        //    var fieldReference = member as FieldReference;
        //    if (fieldReference != null)
        //        return new FieldReference(member.Name, (TypeReference)CreateReference(fieldReference.FieldType, ts), declaringType);

        //    var methodReference = member as MethodReference;
        //    if (methodReference != null)
        //    {
        //        //TODO: more fields may need to be copied
        //        var methodReferenceCopy = new MethodReference(member.Name, (TypeReference)CreateReference(methodReference.SafeReturnType(), ts), declaringType)
        //        {
        //            HasThis = methodReference.HasThis,
        //            ExplicitThis = methodReference.ExplicitThis,
        //            CallingConvention = methodReference.CallingConvention
        //        };

        //        foreach (var parameter in methodReference.Parameters)
        //        {
        //            methodReferenceCopy.Parameters.Add(new ParameterDefinition((TypeReference)CreateReference(methodReference.ResolveGenericType(parameter.ParameterType), ts)));
        //        }

        //        return methodReferenceCopy;
        //    }

        //    throw new NotSupportedException("Not supported member type " + member.GetType().FullName);
        //}
    }
}