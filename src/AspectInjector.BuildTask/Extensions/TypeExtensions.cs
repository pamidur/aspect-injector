using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.BuildTask.Extensions
{
    internal static class TypeExtensions
    {
        public static TypeReference MakeGenericType(this TypeReference self, params TypeReference[] arguments)
        {
            if (self.GenericParameters.Count != arguments.Length)
                throw new ArgumentException();

            var instance = new GenericInstanceType(self);
            foreach (var argument in arguments)
                instance.GenericArguments.Add(argument);

            return instance;
        }

        public static bool BelongsToAssembly(this TypeReference tr, string publicKey)
        {
            var td = tr.Resolve();
            if (td == null)
                return false;

            var token = td.Module.Assembly.Name.PublicKeyToken;
            if (token == null)
                return false;

            return BitConverter.ToString(token)
                .Replace("-", string.Empty).ToLowerInvariant() == publicKey.ToLowerInvariant();
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

        public static IEnumerable<T> GetInterfaceTreeMembers<T>(this TypeReference typeReference, Func<TypeDefinition, IEnumerable<T>> selector)
        {
            var definition = typeReference.Resolve();
            if (!definition.IsInterface)
                throw new NotSupportedException(typeReference.Name + " should be an interface");

            var members = selector(definition);

            if (definition.Interfaces.Count > 0)
                members = members.Concat(definition.Interfaces.Select(i => selector(i.Resolve())).Aggregate<IEnumerable<T>>((a, b) => a.Concat(b)));

            return members;
        }

        public static IEnumerable<TypeDefinition> GetClassesTree(this TypeDefinition type)
        {
            yield return type;

            foreach (var nestedType in type.NestedTypes
                .Where(t => t.IsClass)
                .SelectMany(t => GetClassesTree(t)))
            {
                yield return nestedType;
            }
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
            var resolvedType = typeReference.Module.Import(type);
            return typeReference.IsTypeOf(resolvedType);
        }

        public static bool IsTypeOf(this TypeReference typeReference1, TypeReference typeReference2)
        {
            //see https://www.mail-archive.com/mono-cecil@googlegroups.com/msg01520.html for more info

            //todo: find out correct way to compare generic params

            if (typeReference1.IsGenericParameter && typeReference2.IsGenericParameter)
                return typeReference1 == typeReference2;

            if (typeReference1.IsGenericParameter || typeReference2.IsGenericParameter)
                return false;

            var tr1UniqueString = typeReference1.FullName + "@" + typeReference1.Resolve().Module.Assembly.Name.Name.ToLowerInvariant();
            var tr2UniqueString = typeReference2.FullName + "@" + typeReference2.Resolve().Module.Assembly.Name.Name.ToLowerInvariant();

            //var td1 = typeReference1.Resolve();
            //var td2 = typeReference2.Resolve();

            //var tr1UniqueString = typeReference1.FullName + "@" + (td1 == null ? td1.Module.Assembly.Name.Name.ToLowerInvariant() : "");
            //var tr2UniqueString = typeReference2.FullName + "@" + (td2 == null ? td2.Module.Assembly.Name.Name.ToLowerInvariant() : "");

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