using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.BuildTask.Extensions
{
    internal static class TypeExtensions
    {
        public static bool BelongsToAssembly(this TypeReference tr, string publicKey)
        {
            return BitConverter.ToString(tr.Resolve().Module.Assembly.Name.PublicKeyToken).Replace("-", "").ToLowerInvariant() == publicKey.ToLowerInvariant();
        }

        public static MethodDefinition GetInterfaceImplementation(this TypeReference typeReference, MethodDefinition interfaceMethodDefinition)
        {
            if (interfaceMethodDefinition.DeclaringType.Resolve().IsInterface == false)
                throw new InvalidOperationException(interfaceMethodDefinition.DeclaringType.Name + "." + interfaceMethodDefinition.Name + " is not an interface member.");

            if (!typeReference.ImplementsInterface(interfaceMethodDefinition.DeclaringType))
                throw new InvalidOperationException(typeReference.Name + " doesn't implement " + interfaceMethodDefinition.DeclaringType.Name);

            var typeDefinition = typeReference.Resolve();

            return typeDefinition.Methods.SingleOrDefault(m => m.IsInterfaceImplementation(interfaceMethodDefinition)) ?? typeDefinition.BaseType.GetInterfaceImplementation(interfaceMethodDefinition);
        }

        public static IEnumerable<TypeReference> GetInterfacesTree(this TypeReference typeReference)
        {
            var definition = typeReference.Resolve();
            if (!definition.IsInterface)
                throw new NotSupportedException(typeReference.Name + " should be an interface");

            return new[] { definition }.Concat(definition.Interfaces);
        }

        public static IEnumerable<T> GetInterfaceTreeMemebers<T>(this TypeReference typeReference, Func<TypeDefinition, IEnumerable<T>> selector)
        {
            var definition = typeReference.Resolve();
            if (!definition.IsInterface)
                throw new NotSupportedException(typeReference.Name + " should be an interface");

            var members = selector(definition);

            if (definition.Interfaces.Count > 0)
                members = members.Concat(definition.Interfaces.Select(i => selector(i.Resolve())).Aggregate<IEnumerable<T>>((a, b) => a.Concat(b)));

            return members;
        }

        public static bool HasType(this IEnumerable<TypeReference> typeReferences, Type type)
        {
            return typeReferences.Any(tr => tr.IsTypeOf(type));
        }

        public static bool ImplementsInterface(this TypeReference typeReference, TypeReference @interface)
        {
            TypeDefinition typeDefinition = typeReference.Resolve();
            return typeDefinition.Interfaces.Any(i => i.IsTypeOf(@interface)) || (typeDefinition.BaseType != null && typeDefinition.BaseType.ImplementsInterface(@interface));
        }

        public static bool IsTypeOf(this TypeReference typeReference, Type type)
        {
            var resolvedType = typeReference.Module.TypeSystem.ResolveType(type);
            return typeReference.IsTypeOf(resolvedType);
        }

        public static bool IsTypeOf(this TypeReference typeReference1, TypeReference typeReference2)
        {
            //see https://www.mail-archive.com/mono-cecil@googlegroups.com/msg01520.html for more info

            var tr1UniqueString = typeReference1.FullName + "@" + typeReference1.Resolve().Module.Assembly.Name.Name.ToLowerInvariant();
            var tr2UniqueString = typeReference2.FullName + "@" + typeReference2.Resolve().Module.Assembly.Name.Name.ToLowerInvariant();

            return tr1UniqueString == tr2UniqueString;
        }

        public static TypeDefinition ResolveBaseType(this TypeDefinition type)
        {
            if (type == null)
                return null;

            var baseType = type.BaseType;
            if (baseType == null)
                return null;

            return baseType.Resolve();
        }
    }
}