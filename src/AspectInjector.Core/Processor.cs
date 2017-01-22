using AspectInjector.Core.Services;
using Mono.Cecil;
using System.IO;

namespace AspectInjector.Core
{
    public class Processor : ServiceBase
    {
        private readonly AssemblyProcessor _asmProcessor;

        protected Processor(Logger logger, AssemblyProcessor asmProcessor) : base(logger)
        {
            _asmProcessor = asmProcessor;
        }

        public void Process(string assemblyFile, IAssemblyResolver resolver)
        {
            Log.LogInformation($"Aspect Injector has started for {Path.GetFileName(assemblyFile)}");

            var pdbPresent = AreSymbolsFound(assemblyFile);
            var assembly = ReadAssembly(assemblyFile, resolver, pdbPresent);

            _asmProcessor.ProcessAssembly(assembly);

            if (!Log.IsErrorThrown)
            {
                Log.LogInformation("Assembly has been patched.");
                WriteAssembly(assembly, assemblyFile, pdbPresent);
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

            Log.LogInformation("Assembly has been read.");

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

            Log.LogInformation("Assembly has been written.");
        }

        private bool AreSymbolsFound(string dllPath)
        {
            var pdbPath = Path.Combine(Path.GetDirectoryName(dllPath), Path.GetFileNameWithoutExtension(dllPath) + ".pdb");

            if (File.Exists(pdbPath))
                return true;

            Log.LogInformation($"Symbols not found on {pdbPath}. Proceeding without...");
            return false;
        }
    }
}