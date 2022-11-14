using Mono.Cecil;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FluentIL.Resolvers
{
    
    public class KnownReferencesAssemblyResolver : CachedAssemblyResolver
    {
        private readonly List<string> _references = new List<string>();

        public void AddReference(string assemblyPath)
        {
            _references.Add(assemblyPath);
        }

        protected override AssemblyDefinition LookupAssembly(AssemblyNameReference name, ReaderParameters parameters)
        {
            var extensions = name.IsWindowsRuntime ? new[] { ".winmd", ".dll" } : new[] { ".exe", ".dll" };

            foreach (var extension in extensions)
            {
                string file = _references.FirstOrDefault(r => Path.GetFileName(r) == name.Name + extension);
                if (file == null || !File.Exists(file))
                    continue;
                try
                {
                    return GetAssembly(file, parameters);
                }
                catch (System.BadImageFormatException)
                {
                    //skip assemblies we cannot load
                }
            }

            return base.LookupAssembly(name, parameters);
        }

        private AssemblyDefinition GetAssembly(string file, ReaderParameters parameters)
        {
            if (parameters.AssemblyResolver == null)
                parameters.AssemblyResolver = this;

            return ModuleDefinition.ReadModule(file, parameters).Assembly;
        }
    }
}