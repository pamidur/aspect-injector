using dnlib.DotNet;
using System;
using System.Linq;

namespace FluentIL.Extensions;

public static class CecilReferenceExtensions
{
    public static bool Match(this IType tr1, IType tr2)
    {
        if (tr1 == null || tr2 == null)
            return false;

        if (tr1 == tr2) return true;

        return tr1.FullName == tr2.FullName;
    }

    public static bool Match(this IType tr1, StandardType type)
    {
        if (tr1 == null || type == null)
            return false;

        return tr1.FullName == type.ToString();
    }

    public static bool Implements(this ITypeDefOrRef tr, IType @interface)
    {
        var td = tr.ResolveTypeDef();
        var ti = @interface;

        return td.Interfaces.Any(i => i.Interface.Match(ti)) || (td.BaseType != null && td.BaseType.Implements(ti));
    }

    public static bool IsAsync(this IMethodDefOrRef m)
    {
        return m.CustomAttributes.Any(a => a.AttributeType.Match(StandardType.AsyncStateMachineAttribute));
    }

    public static bool IsIterator(this IMethodDefOrRef m)
    {
        return m.CustomAttributes.Any(a => a.AttributeType.Match(StandardType.IteratorStateMachineAttribute));
    }

    public static bool IsUnsafe(this IMethod m)
    {
        return m.MethodSig.RetType.IsPointer || m.MethodSig.Params.Any(p => p.IsPointer);
    }

    public static bool IsNormalMethod(this MethodDef m)
    {
        return !m.IsAddOn && !m.IsRemoveOn && !m.IsSetter && !m.IsGetter && !m.IsConstructor;
    }

    public static bool IsExplicitImplementationOf(this MethodDef m, IMethod ifaceMethod)
    {
        if (m.Overrides.Any(o => o.MethodDeclaration.FullName == ifaceMethod.FullName))
            return true;

        return false;
    }

    public static IType GetEnumType(this TypeDef enumtype)
    {
        if (!enumtype.IsEnum)
            throw new InvalidOperationException($"{enumtype.Name} is not enum");

        return enumtype.Fields.First(f => f.Name == "value__").FieldType;
    }
}