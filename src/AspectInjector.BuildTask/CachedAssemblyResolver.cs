using AspectInjector.Broker;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.BuildTask
{
    [ComVisible(false)]
    public class CachedAssemblyResolver : DefaultAssemblyResolver
    {
        private StrictAssemblyResolver _strictResolver;
        private string _cacheFolder;

        public CachedAssemblyResolver(string cacheFolder)
        {
            _cacheFolder = cacheFolder;
            _strictResolver = new StrictAssemblyResolver();
            _strictResolver.RegisterAssembly(typeof(AspectAttribute).Assembly.Location);
        }

        public void RegisterAssembly(string assemblyPath)
        {
            var origFile = Path.Combine(_cacheFolder, Path.GetFileName(assemblyPath));

            var assmDefinition = AssemblyDefinition.ReadAssembly(
                File.Exists(origFile) ? origFile : assemblyPath,
                new ReaderParameters
                {
                    AssemblyResolver = _strictResolver,
                    ReadingMode = Mono.Cecil.ReadingMode.Deferred
                });
            RegisterAssembly(assmDefinition);
        }
    }
}
