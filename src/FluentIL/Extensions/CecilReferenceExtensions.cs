using Mono.Cecil;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FluentIL.Extensions
{
    public static class CecilReferenceExtensions
    {
        private static readonly TypeReference _asyncStateMachineAttribute = StandardTypes.GetType(typeof(AsyncStateMachineAttribute));
        private static readonly TypeReference _iteratorStateMachineAttribute = StandardTypes.GetType(typeof(IteratorStateMachineAttribute));

        public static bool Match(this TypeReference tr1, TypeReference tr2)
        {
            if (tr1 == null || tr2 == null)
                return false;

            if (tr1 == tr2) return true;

            return tr1.FullName == tr2.FullName;
        }

        public static bool Implements(this TypeReference tr, TypeReference @interface)
        {
            var td = tr.Resolve();
            var ti = @interface;

            return td.Interfaces.Any(i => i.InterfaceType.Match(ti)) || (td.BaseType != null && td.BaseType.Implements(ti));
        }

        public static bool IsAsync(this MethodDefinition m)
        {
            return m.CustomAttributes.Any(a => a.AttributeType.Match(_asyncStateMachineAttribute));
        }

        public static bool IsIterator(this MethodDefinition m)
        {
            return m.CustomAttributes.Any(a => a.AttributeType.Match(_iteratorStateMachineAttribute));
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