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
            return m.CustomAttributes.Any(a => a.AttributeType.FullName == WellKnownTypes.AsyncStateMachineAttribute);
        }

        public static bool IsIterator(this MethodDefinition m)
        {
            return m.CustomAttributes.Any(a => a.AttributeType.FullName == WellKnownTypes.IteratorStateMachineAttribute);
        }

        public static bool IsUnsafe(this MethodDefinition m)
        {
            return m.ReturnType.IsPointer || m.Parameters.Any(p => p.ParameterType.IsPointer);
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
                    if (!m.Parameters[i].ParameterType.Match(ifaceMethod.ResolveGenericType(ifaceMethodDef.Parameters[i].ParameterType)))
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
    }
}