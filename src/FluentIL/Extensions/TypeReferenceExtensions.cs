using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace FluentIL.Extensions
{
    public static class TypeReferenceExtensions
    {
        public static bool BelongsToAssembly(this TypeReference tr, byte[] publicKey)
        {
            var td = tr.Resolve();
            if (td == null)
                return false;

            var token = td.Module.Assembly.Name.PublicKeyToken;
            if (token == null)
                return false;

            return token.SequenceEqual(publicKey);
        }

        public static bool Match(this TypeReference tr1, TypeReference tr2)
        {
            if (tr1 == null || tr2 == null)
                return false;

            return tr1.FullName == tr2.FullName;
        }

        public static IEnumerable<TypeDefinition> GetTypesTree(this TypeDefinition type)
        {
            yield return type;

            foreach (var nestedType in type.NestedTypes

                .SelectMany(t => GetTypesTree(t)))
            {
                yield return nestedType;
            }
        }

        public static bool Implements(this TypeReference tr, TypeReference @interface)
        {
            var td = tr.Resolve();
            var ti = @interface;

            return td.Interfaces.Any(i => i.InterfaceType.Match(ti)) || (td.BaseType != null && td.BaseType.Implements(ti));
        }
    }
}