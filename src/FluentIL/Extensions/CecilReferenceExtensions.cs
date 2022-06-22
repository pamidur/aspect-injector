using Mono.Cecil;
using System;
using System.Linq;

namespace FluentIL.Extensions
{
    public static class CecilReferenceExtensions
    {
        public static bool Match(this TypeReference tr1, TypeReference tr2)
        {
            if (tr1 == null || tr2 == null)
                return false;

            if (tr1 == tr2) return true;

            return tr1.FullName == tr2.FullName;
        }

        public static bool Match(this TypeReference tr1, StandardType type)
        {
            if (tr1 == null || type == null)
                return false;

            return tr1.FullName == type.ToString();
        }

        public static bool Implements(this TypeReference tr, TypeReference @interface)
        {
            var td = tr.Resolve();
            var ti = @interface;

            return td.Interfaces.Any(i => i.InterfaceType.Match(ti)) || (td.BaseType != null && td.BaseType.Implements(ti));
        }

        public static bool IsAsync(this MethodDefinition m)
        {
            return m.CustomAttributes.Any(a => a.AttributeType.Match(StandardType.AsyncStateMachineAttribute));
        }

        public static bool IsIterator(this MethodDefinition m)
        {
            return m.CustomAttributes.Any(a => a.AttributeType.Match(StandardType.IteratorStateMachineAttribute));
        }

        public static bool IsUnsafe(this MethodDefinition m)
        {
            return m.ReturnType.IsPointer || m.Parameters.Any(p => p.ParameterType.IsPointer);
        }

        public static bool IsNormalMethod(this MethodDefinition m)
        {
            return !m.IsAddOn && !m.IsRemoveOn && !m.IsSetter && !m.IsGetter && !m.IsConstructor;
        }

        public static bool IsExplicitImplementationOf(this MethodDefinition m, MethodReference ifaceMethod)
        {
            if (m.Overrides.Any(o => o.FullName == ifaceMethod.FullName))
                return true;

            return false;
        }     
        
        public static TypeReference GetEnumType(this TypeDefinition enumtype)
        {
            if (!enumtype.IsEnum)
                throw new InvalidOperationException($"{enumtype.Name} is not enum");

            return enumtype.Fields.First(f => f.Name == "value__").FieldType;
        }
    }
}