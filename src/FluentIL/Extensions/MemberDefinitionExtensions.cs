using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
using System.Linq;

namespace FluentIL.Extensions
{
    public static class MemberDefinitionExtensions
    {
        public static PropertyDefinition FindProperty(this TypeDefinition typeDef, string name)
        {
            var result = typeDef.Properties.FirstOrDefault(p => p.Name == name);

            if(result == null && typeDef.BaseType != null)
            {
                result = typeDef.BaseType.Resolve().FindProperty(name);                
            }

            return result;
        }

        public static FieldDefinition FindField(this TypeDefinition typeDef, string name)
        {
            var result = typeDef.Fields.FirstOrDefault(p => p.Name == name);

            if (result == null && typeDef.BaseType != null)
            {
                result = typeDef.BaseType.Resolve().FindField(name);
            }

            return result;
        }

        public static bool IsCallCompatible(this MemberReference member)
        {
            if (member is TypeReference typeRef)
                return !typeRef.HasGenericParameters;

            if (member is MethodReference methodRef)
                return !methodRef.HasGenericParameters &&
                methodRef.DeclaringType.IsCallCompatible();

            if (member is FieldReference fieldRef)
                return fieldRef.DeclaringType.IsCallCompatible();

            throw new NotSupportedException($"{member.GetType().Name} is not callable");
        }

        public static TypeReference MakeSelfReference(this TypeDefinition definition)
        {
            TypeReference reference = definition;
            if (definition.HasGenericParameters)
                reference = definition.MakeGenericInstanceType(definition.GenericParameters.ToArray());
            return reference;
        }

        public static FieldReference MakeReference(this FieldDefinition definition, TypeReference ownerTypeRef)
        {
            if (!ownerTypeRef.IsCallCompatible())
                throw new ArgumentException($"Owner type is not call compatible", nameof(ownerTypeRef));

            return new FieldReference(definition.Name, ownerTypeRef.Module.ImportReference(definition.FieldType), ownerTypeRef);
        }

        public static MethodReference MakeReference(this MethodReference definition, TypeReference ownerTypeRef)
        {
            if (!ownerTypeRef.IsCallCompatible())
                throw new ArgumentException($"Owner type is not call compatible", nameof(ownerTypeRef));

            var reference = new MethodReference(definition.Name, ownerTypeRef.Module.ImportReference(definition.ReturnType), ownerTypeRef);

            reference.HasThis = definition.HasThis;
            reference.ExplicitThis = definition.ExplicitThis;
            reference.CallingConvention = definition.CallingConvention;

            definition.GenericParameters.CloneTo(reference);

            foreach (var par in definition.Parameters)
                reference.Parameters.Add(par);

            return reference;
        }

        public static GenericInstanceMethod MakeGenericInstanceMethod(this MethodReference self, params TypeReference[] arguments)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));

            if (arguments == null)
                throw new ArgumentNullException(nameof(arguments));

            if (arguments.Length == 0)
                throw new ArgumentException("Expected non-zero arguments");

            if (self.GenericParameters.Count != arguments.Length)
                throw new ArgumentException("Expected arguments == method.genericparams.count");

            var genericInstanceMethod = new GenericInstanceMethod(self);
            foreach (TypeReference item in arguments)
            {
                genericInstanceMethod.GenericArguments.Add(item);
            }

            return genericInstanceMethod;
        }
    }
}
