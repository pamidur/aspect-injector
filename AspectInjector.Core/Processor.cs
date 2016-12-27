using AspectInjector.Core.Configuration;
using AspectInjector.Core.Utils;
using Mono.Cecil;
using System.IO;

namespace AspectInjector.Core
{
    public class Processor
    {
        private readonly ProcessingConfiguration _config;

        public Processor(ProcessingConfiguration config)
        {
            _config = config;
        }

        public void Process(string assemblyFile, IAssemblyResolver resolver)
        {
            _config.Log.LogInformation($"Aspect Injector has started for {Path.GetFileName(assemblyFile)}");

            var context = _config.CreateContext(ReadAssembly(assemblyFile, resolver), resolver);

            context.Services.AssemblyProcessor.ProcessAssembly(context.Assembly);

            _config.Log.LogInformation("Assembly has been patched.");

            WriteAssembly(context.Assembly, assemblyFile);
        }

        private AssemblyDefinition ReadAssembly(string assemblyFile, IAssemblyResolver resolver)
        {
            var assembly = AssemblyDefinition.ReadAssembly(assemblyFile,
                new ReaderParameters
                {
                    ReadingMode = ReadingMode.Deferred
                });

            assembly = resolver.Resolve(assembly.Name, new ReaderParameters
            {
                ReadingMode = ReadingMode.Deferred,
                AssemblyResolver = resolver,
                ReadSymbols = AreSymbolsFound(assemblyFile)
            });

            _config.Log.LogInformation("Assembly has been read.");

            return assembly;
        }

        private void WriteAssembly(AssemblyDefinition assembly, string path)
        {
            assembly.Write(path,
                new WriterParameters()
                {
                    WriteSymbols = AreSymbolsFound(path),
                    ////StrongNameKeyPair = Sing && !DelaySing ? new StrongNameKeyPair(StrongKeyPath) : null
                });

            _config.Log.LogInformation("Assembly has been written.");
        }

        private bool AreSymbolsFound(string dllPath)
        {
            return File.Exists(Path.Combine(Path.GetDirectoryName(dllPath), Path.GetFileNameWithoutExtension(dllPath) + ".pdb"));
        }
    }
}