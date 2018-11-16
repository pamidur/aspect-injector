using AspectInjector.Core.Contracts;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AspectInjector.Core
{
    public class Processor
    {
        private readonly IAspectReader _aspectExtractor;
        private readonly IAspectWeaver _aspectWeaver;
        private readonly IEnumerable<IEffectWeaver> _effectWeavers;
        private readonly IInjectionReader _injectionCollector;
        private readonly ILogger _log;

        public Processor(
            IAspectReader aspectExtractor,
            IInjectionReader injectionCollector,
            IAspectWeaver aspectWeaver,
            IEnumerable<IEffectWeaver> effectWeavers,
            ILogger logger)
        {
            _aspectExtractor = aspectExtractor;
            _injectionCollector = injectionCollector;
            _aspectWeaver = aspectWeaver;
            _effectWeavers = effectWeavers;
            _log = logger;
        }

        public void Process(string assemblyFile, IAssemblyResolver resolver, bool optimize)
        {
            _log.LogInfo($"Aspect Injector has started for {Path.GetFileName(assemblyFile)}");

            var pdbPresent = AreSymbolsFound(assemblyFile);
            var assembly = ReadAssembly(assemblyFile, resolver, pdbPresent);

            ProcessAssembly(assembly, optimize);

            if (!_log.IsErrorThrown)
            {
                _log.LogInfo("Assembly has been patched.");
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

            _log.LogInfo("Assembly has been read.");

            return assembly;
        }

        private void WriteAssembly(AssemblyDefinition assembly, string path, bool writeSymbols)
        {
            var origOut = Path.GetDirectoryName(path);
            var tempDir = Path.Combine(origOut, "aspect_compile");
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }

            Directory.CreateDirectory(tempDir);

            var fileName = Path.Combine(tempDir, Path.GetFileName(path));
            var pdbName = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(path) + ".pdb");

            using (var fileStream = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                assembly.Write(fileStream,
                    new WriterParameters()
                    {
                        WriteSymbols = writeSymbols,
                        ////StrongNameKeyPair = Sing && !DelaySing ? new StrongNameKeyPair(StrongKeyPath) : null
                    });

                assembly.MainModule.SymbolReader.Dispose();
                assembly.Dispose();
                assembly = null;
            }

            foreach (var file in Directory.GetFiles(tempDir))
            {
                var origFile = Path.Combine(origOut, Path.GetFileName(file));
                File.Replace(file, origFile, origFile + ".old");
            }

            Directory.Delete(tempDir, true);

            _log.LogInfo("Assembly has been written.");
        }

        private bool AreSymbolsFound(string dllPath)
        {
            var pdbPath = Path.Combine(Path.GetDirectoryName(dllPath), Path.GetFileNameWithoutExtension(dllPath) + ".pdb");

            if (File.Exists(pdbPath))
            {
                return true;
            }

            _log.LogInfo($"Symbols not found on {pdbPath}. Proceeding without...");
            return false;
        }

        public void ProcessAssembly(AssemblyDefinition assembly, bool optimize)
        {
            var aspects = _aspectExtractor.ReadAll(assembly);
            _log.LogInfo($"Found {aspects.Count} aspects");

            var injections = _injectionCollector.ReadAll(assembly).ToList();
            _log.LogInfo($"Found {injections.Count} injections");

            if (_log.IsErrorThrown)            
                return;            

            if (aspects.Count != 0)
            {
                _log.LogInfo($"Processing aspects...");
                foreach (var aspect in aspects)
                    _aspectWeaver.WeaveGlobalAssests(aspect);
            }

            if (injections.Count != 0)
            {
                _log.LogInfo($"Processing injections...");

                foreach (var injector in _effectWeavers.OrderByDescending(i => i.Priority))
                {
                    _log.LogInfo($"Executing {injector.GetType().Name}...");

                    foreach (var prioritizedInjections in injections.GroupBy(i => i.Priority).OrderByDescending(a => a.Key).ToList())
                        foreach (var injection in prioritizedInjections.OrderByDescending(i => i.Effect.Priority))
                            if (injector.CanWeave(injection))
                            {
                                injector.Weave(injection);
                                injections.Remove(injection);
                            }
                }

                foreach (var injection in injections)
                    _log.LogError(CompilationMessage.From($"Couldn't find weaver for {injection.ToString()}", injection.Target));
            }

            if (optimize)
                _log.LogInfo($"Cleanup and optimize...");
            else
                _log.LogInfo($"Cleanup...");

            foreach (var module in assembly.Modules)
            {
                if (optimize)                
                    EditorFactory.Optimize(module);
                
                EditorFactory.CleanUp(module);
            }
        }

        private List<Injection> ExcludeAspectInjections(IEnumerable<Injection> injections, IEnumerable<AspectDefinition> aspects)
        {
            var aspectTypes = new HashSet<TypeDefinition>(aspects.Select(a => a.Host));
            return injections.Where(i => !aspectTypes.Contains(i.Target.DeclaringType)).ToList();
        }
    }
}