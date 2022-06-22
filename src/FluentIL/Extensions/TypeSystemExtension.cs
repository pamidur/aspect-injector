using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentIL.Extensions
{
    public static class TypeSystemExtension
    {
        private static readonly Dictionary<(StandardType, ModuleDefinition), TypeReference> _typeCache = new Dictionary<(StandardType, ModuleDefinition), TypeReference>();
        private static readonly List<(string, ModuleDefinition)> _invalidHints = new List<(string, ModuleDefinition)>();
        public static TypeReference ImportStandardType(this ModuleDefinition module, StandardType standardType)
        {
            var tr = GetTypeRef(standardType, module);
            return module.ImportReference(tr);
        }

        private static TypeReference GetTypeRef(StandardType type, ModuleDefinition module)
        {
            if (!_typeCache.TryGetValue((type, module), out var tr))
            {
                var lib = GetMetadataScope(module, type);
                tr = new TypeReference(type.Namespace, type.Name, module, lib, type.IsValueType);    

                if (type.Elements.Count > 0)
                {
                    for (int i = 0; i < type.Elements.Count; i++)
                    {
                        tr.GenericParameters.Add(new GenericParameter(tr));
                    }

                    tr = module.ImportReference(tr);
                    tr = tr.MakeGenericInstanceType(type.Elements.Select(e => GetTypeRef(e, module)).ToArray());
                }

                tr = module.ImportReference(tr);
                if (tr.Resolve() == null)
                    throw new InvalidOperationException($"'{type}' not a part of standard lib.");

                if (type.IsArray)
                    tr = new ArrayType(tr);

                _typeCache[(type, module)] = tr;
            }

            return tr;
        }

        private static IMetadataScope GetMetadataScope(ModuleDefinition module, StandardType type)
        {
            if (type.AssemblyHints.Count == 0)
                return module.TypeSystem.CoreLibrary;

            foreach (var hint in type.AssemblyHints)
            {
                if (_invalidHints.Contains((hint, module)))
                    continue;

                var assmName = new AssemblyNameReference(hint, new Version());

                AssemblyDefinition assm = null;
                try
                {
                    assm = module.AssemblyResolver.Resolve(assmName);
                }
                catch
                {
                    _invalidHints.Add((hint, module));
                    continue;
                }

                var reftype = assm.MainModule.GetTypes().FirstOrDefault(t => t.Namespace == type.Namespace && t.Name == type.Name);
                if (reftype == null) continue;

                assmName = AssemblyNameReference.Parse(assm.FullName);

                if (!module.AssemblyReferences.Contains(assmName, AssemblyNameReferenceComparer.Instance))
                    module.AssemblyReferences.Add(assmName);

                return assmName;
            }

            return module.TypeSystem.CoreLibrary;
        }
    }

    internal class AssemblyNameReferenceComparer : IEqualityComparer<AssemblyNameReference>
    {
        public static readonly AssemblyNameReferenceComparer Instance = new AssemblyNameReferenceComparer();
        public bool Equals(AssemblyNameReference x, AssemblyNameReference y)
        {
            return x.FullName == y.FullName;
        }

        public int GetHashCode(AssemblyNameReference obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
