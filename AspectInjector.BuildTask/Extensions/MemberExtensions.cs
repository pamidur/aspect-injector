using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace AspectInjector.BuildTask.Extensions
{
    internal static class MemberExtensions
    {
        public static Instruction FindBaseClassCtorCall(this MethodDefinition md)
        {
            if (!md.IsConstructor)
                throw new Exception(md.ToString() + " is not ctor.");

            if (md.DeclaringType.IsValueType)
                return md.Body.Instructions.First();

            var point = md.Body.Instructions.FirstOrDefault(
                i => i.OpCode == OpCodes.Ldarg_0
                    && i.Next != null && i.Next.OpCode == OpCodes.Call && i.Next.Operand is MethodReference
                    && ((MethodReference)i.Next.Operand).Resolve().IsConstructor
                    && ((MethodReference)i.Next.Operand).DeclaringType.IsTypeOf(md.DeclaringType.BaseType));

            if (point == null)
                throw new Exception("Cannot find base class ctor call");

            return point.Next.Next;
        }

        public static bool IsInterfaceImplementation(this MethodDefinition method, MethodReference overridden)
        {
            if (method.IsExplicitInterfaceImplementation(overridden))
                return true;

            if (method.IsImplicitInterfaceImplementation(overridden))
                return true;

            // new slot method cannot override any base classes' method by convention:
            if (method.IsNewSlot)
                return false;

            // check base-type overrides
            var baseMethod = method.GetOriginalBaseMethod();

            return baseMethod.IsImplicitInterfaceImplementation(overridden) || baseMethod.IsExplicitInterfaceImplementation(overridden);
        }

        public static bool IsExplicitInterfaceImplementation(this MethodDefinition method, MethodReference overridden)
        {
            return method.Overrides.Any(o => o.IsMemberReferenceOf(overridden));
        }

        public static bool SignatureMatches(this MethodReference methodReference1, MethodReference methodReference2)
        {
            if (!methodReference1.MethodReturnType.ReturnType.IsTypeOf(methodReference2.MethodReturnType.ReturnType))
                return false;

            for (int i = 0; i < methodReference1.Parameters.Count; i++)
                if (!methodReference1.Parameters[i].ParameterType.IsTypeOf(methodReference2.Parameters[i].ParameterType))
                    return false;

            return true;
        }

        public static bool IsMemberReferenceOf(this MemberReference memberReference1, MemberReference memberReference2)
        {
            return memberReference1.Name == memberReference2.Name && memberReference1.DeclaringType.IsTypeOf(memberReference1.DeclaringType);
        }

        public static MethodDefinition GetBaseMethod(this MethodDefinition self)
        {
            if (self == null)
                throw new ArgumentNullException("self");

            if (!self.IsVirtual)
                return self;

            var baseType = self.DeclaringType.ResolveBaseType();
            while (baseType != null)
            {
                var @base = baseType.Methods.FirstOrDefault(m => m.SignatureMatches(self));
                if (@base != null)
                    return @base;

                baseType = baseType.ResolveBaseType();
            }

            return self;
        }

        public static MethodDefinition GetOriginalBaseMethod(this MethodDefinition self)
        {
            if (self == null)
                throw new ArgumentNullException("self");

            while (true)
            {
                var @base = self.GetBaseMethod();
                if (@base == self)
                    return self;

                self = @base;
            }
        }

        private static bool IsImplicitInterfaceImplementation(this MethodDefinition method, MethodReference overridden)
        {
            // check that the 'overridden' method is iface method and the iface is implemented by method.DeclaringType
            if (overridden.DeclaringType.Resolve().IsInterface == false ||
                !method.DeclaringType.ImplementsInterface(overridden.DeclaringType))
            {
                return false;
            }

            // check whether the type contains some other explicit implementation of the method
            if (method.DeclaringType.Methods.SelectMany(m => m.Overrides).Any(m => m.IsMemberReferenceOf(overridden)))
            {
                // explicit implementation -> no implicit implementation possible
                return false;
            }

            // now it is enough to just match the signatures and names:
            return method.Name == overridden.Name && method.SignatureMatches(overridden);
        }
    }
}