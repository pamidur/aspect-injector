using AspectInjector.Core.Configuration;
using AspectInjector.Core.Contracts;
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

            var pdbPresent = AreSymbolsFound(assemblyFile);

            var context = _config.CreateContext(ReadAssembly(assemblyFile, resolver, pdbPresent), resolver);
            context.Services.AssemblyProcessor.ProcessAssembly(context.Assembly);

            if (!context.Services.Log.IsErrorThrown)
            {
                _config.Log.LogInformation("Assembly has been patched.");
                WriteAssembly(context.Assembly, assemblyFile, pdbPresent);
            }
        }

        private AssemblyDefinition ReadAssembly(string assemblyFile, IAssemblyResolver resolver, bool readSymbols)
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
                ReadSymbols = readSymbols
            });

            _config.Log.LogInformation("Assembly has been read.");

            return assembly;
        }

        private void WriteAssembly(AssemblyDefinition assembly, string path, bool writeSymbols)
        {
            assembly.Write(path,
                new WriterParameters()
                {
                    WriteSymbols = writeSymbols,
                    ////StrongNameKeyPair = Sing && !DelaySing ? new StrongNameKeyPair(StrongKeyPath) : null
                });

            _config.Log.LogInformation("Assembly has been written.");
        }

        private bool AreSymbolsFound(string dllPath)
        {
            var pdbPath = Path.Combine(Path.GetDirectoryName(dllPath), Path.GetFileNameWithoutExtension(dllPath) + ".pdb");

            if (File.Exists(pdbPath))
                return true;

            _config.Log.LogInformation($"Symbols not found on {pdbPath}. Proceeding without...");
            return false;
        }
    }
}