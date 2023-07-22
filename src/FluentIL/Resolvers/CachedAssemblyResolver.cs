using dnlib.DotNet;
using System;
using System.Collections.Generic;

namespace FluentIL.Resolvers
{
    public class CachedAssemblyResolver : AssemblyResolver
    {
        private readonly Dictionary<string, AssemblyDef> _cache = new Dictionary<string, AssemblyDef>();

        public AssemblyDef Resolve(IAssembly name, ModuleDef sourceModule)
        {
            var result = _cache.ContainsKey(name.FullName) ? _cache[name.FullName] : null;

            if (result == null)
            {
                result = LookupAssembly(name, parameters);
                _cache[name.FullName] = result;
            }

            return result;
        }        

        protected virtual AssemblyDefinition LookupAssembly(AssemblyNameReference name, ReaderParameters parameters)
        {
            mod = ModuleDefMD.Load(path, moduleContext);
            var asm = mod.Assembly;
            if (asm is not null && asmComparer.CompareClosest(assembly, closest, asm) == 1)
            {
                if (!IsCached(closest) && closest is not null)
                {
                    var closeMod = closest.ManifestModule;
                    if (closeMod is not null)
                        closeMod.Dispose();
                }
                closest = asm;
                mod = null;
            }

            return base.Resolve(name, parameters);
        }

        protected internal void RegisterAssembly(AssemblyDefinition assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            var name = assembly.Name.FullName;
            if (_cache.ContainsKey(name))
                return;

            _cache[name] = assembly;
        }
    }
}