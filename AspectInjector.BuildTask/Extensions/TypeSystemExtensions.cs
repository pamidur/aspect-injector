using Mono.Cecil;
using System;

namespace AspectInjector.BuildTask.Extensions
{
    public static class TypeSystemExtensions
    {
        public static TypeReference ResolveType(this TypeSystem ts, Type type)
        {
            if (type.Namespace != "System")
                throw new NotSupportedException("Cannot resolve type of " + type);

            if (type == typeof(object))
                return ts.Object;

            if (type == typeof(bool))
                return ts.Boolean;

            if (type == typeof(string))
                return ts.String;

            if (type == typeof(int))
                return ts.Int32;

            if (type == typeof(void))
                return ts.Void;

            return new TypeReference(type.Namespace, type.Name, ts.Boolean.Module, ts.Corlib);
        }
    }
}