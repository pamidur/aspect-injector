using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AspectInjector.Core
{
    public class Processor
    {
        private readonly IAspectExtractor _aspectExtractor;
        private readonly IAspectWeaver _assectWeaver;
        private readonly IAssetsCache _cache;
        private readonly IEnumerable<IEffectWeaver> _effectWeavers;
        private readonly IInjectionCollector _injectionCollector;
        private readonly IJanitor _janitor;
        private readonly ILogger _log;

        public Processor(IJanitor janitor, IAspectExtractor aspectExtractor, IAssetsCache cache, IInjectionCollector injectionCollector, IAspectWeaver assectWeaver, IEnumerable<IEffectWeaver> effectWeavers, ILogger logger)
        {
            _aspectExtractor = aspectExtractor;
            _injectionCollector = injectionCollector;
            _cache = cache;
            _assectWeaver = assectWeaver;
            _effectWeavers = effectWeavers;
            _janitor = janitor;
            _log = logger;
        }

        public void Process(string assemblyFile, IAssemblyResolver resolver)
        {
            _log.LogInfo($"Aspect Injector has started for {Path.GetFileName(assemblyFile)}");

            var pdbPresent = AreSymbolsFound(assemblyFile);
            var assembly = ReadAssembly(assemblyFile, resolver, pdbPresent);

            ProcessAssembly(assembly);

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
            assembly.Write(path,
                new WriterParameters()
                {
                    WriteSymbols = writeSymbols,
                    ////StrongNameKeyPair = Sing && !DelaySing ? new StrongNameKeyPair(StrongKeyPath) : null
                });

            _log.LogInfo("Assembly has been written.");
        }

        private bool AreSymbolsFound(string dllPath)
        {
            var pdbPath = Path.Combine(Path.GetDirectoryName(dllPath), Path.GetFileNameWithoutExtension(dllPath) + ".pdb");

            if (File.Exists(pdbPath))
                return true;

            _log.LogInfo($"Symbols not found on {pdbPath}. Proceeding without...");
            return false;
        }

        public void ProcessAssembly(AssemblyDefinition assembly)
        {
            var aspects = _aspectExtractor.Extract(assembly);

            foreach (var aspect in aspects)
                _cache.Cache(aspect);

            var injections = _injectionCollector.Collect(assembly).ToList();

            _janitor.Cleanup(assembly);

            if (_log.IsErrorThrown)
                return;

            _cache.FlushCache(assembly);

            foreach (var aspect in aspects)
                _assectWeaver.WeaveGlobalAssests(aspect);

            foreach (var injector in _effectWeavers.OrderByDescending(i => i.Priority))
            {
                _log.LogInfo($"Executing {injector.GetType().Name}");

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
    }
}