using FluentIL.Common;
using FluentIL.Logging;
using FluentIL.Resolvers;
using Mono.Cecil;
using System.Collections.Generic;
using System.IO;

namespace FluentIL
{
    public abstract class PatcherBase
    {
        protected readonly ILogger _log;

        protected PatcherBase(
            ILogger logger)
        {
            _log = logger;
        }

        public void Process(string assemblyFile, IReadOnlyList<string> references, bool optimize, bool verbose)
        {
            var resolver = GetResolver(assemblyFile, references);
            if (_log.IsErrorThrown) return;

            Process(assemblyFile, resolver, optimize, verbose);
        }

        public void Process(string assemblyFile, IAssemblyResolver resolver, bool optimize, bool verbose)
        {
            if (!File.Exists(assemblyFile))
            {
                _log.Log(GenericErrorRule, $"Target not found: '{assemblyFile}'");
                return;
            }

            if (verbose) _log.Log(GenericInfoRule, $"Started for {Path.GetFileName(assemblyFile)}");

            var pdbPresent = AreSymbolsFound(assemblyFile);
            var assembly = ReadAssembly(assemblyFile, resolver, pdbPresent, verbose);

            var modified = PatchAssembly(assembly, optimize, verbose);

            if (!_log.IsErrorThrown)
            {
                if (modified)
                {
                    if (verbose) _log.Log(GenericInfoRule, "Assembly has been patched.");
                    WriteAssembly(assembly, pdbPresent, verbose);
                }
                else if (verbose) _log.Log(GenericInfoRule, "No patching required.");
            }
        }

        protected virtual IAssemblyResolver GetResolver(string assemblyFile, IReadOnlyList<string> references)
        {
            var resolver = new KnownReferencesAssemblyResolver();

            if (!File.Exists(assemblyFile)) _log.Log(GenericErrorRule, $"Target not found: '{assemblyFile}'");
            else resolver.AddSearchDirectory(Path.GetDirectoryName(assemblyFile));

            foreach (var refr in references)
            {
                if (!File.Exists(refr)) _log.Log(GenericErrorRule, $"Reference not found: '{refr}'");
                else resolver.AddReference(refr);
            }

            return resolver;
        }

        private AssemblyDefinition ReadAssembly(string assemblyFile, IAssemblyResolver resolver, bool readSymbols,bool verbose)
        {
            var assembly = AssemblyDefinition.ReadAssembly(assemblyFile,
                new ReaderParameters
                {
                    ReadingMode = ReadingMode.Deferred
                });

            assembly.Dispose();

            assembly = resolver.Resolve(assembly.Name, new ReaderParameters
            {
                ReadingMode = ReadingMode.Immediate,
                ReadWrite = true,
                AssemblyResolver = resolver,
                ReadSymbols = readSymbols
            });

            if (verbose) _log.Log(GenericInfoRule, "Assembly has been read.");

            return assembly;
        }

        private void WriteAssembly(AssemblyDefinition assembly, bool writeSymbols, bool verbose)
        {
            var param = new WriterParameters();

            if (writeSymbols)
            {
                param.WriteSymbols = true;
            }

            assembly.Write(param);

            if (assembly.MainModule.SymbolReader != null)
                assembly.MainModule.SymbolReader.Dispose();

            assembly.Dispose();

            if (verbose) _log.Log(GenericInfoRule, "Assembly has been written.");
        }

        private bool AreSymbolsFound(string dllPath)
        {
            var pdbPath = Path.Combine(Path.GetDirectoryName(dllPath), Path.GetFileNameWithoutExtension(dllPath) + ".pdb");

            if (File.Exists(pdbPath))
            {
                return true;
            }

            _log.Log(GenericInfoRule, $"Symbols not found on {pdbPath}. Proceeding without...");
            return false;
        }

        protected abstract Rule GenericInfoRule { get; }
        protected abstract Rule GenericErrorRule { get; }

        protected abstract bool PatchAssembly(AssemblyDefinition assembly, bool optimize, bool verbose);
    }
}