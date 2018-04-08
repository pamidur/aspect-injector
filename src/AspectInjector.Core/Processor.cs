using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AspectInjector.Core
{
    public class Processor
    {
        private readonly IAspectExtractor _aspectExtractor;
        private readonly IAspectWeaver _aspectWeaver;
        private readonly IAssetsCache _cache;
        private readonly IEnumerable<IEffectWeaver> _effectWeavers;
        private readonly IInjectionCollector _injectionCollector;
        private readonly IJanitor _janitor;
        private readonly ILogger _log;

        public Processor(IJanitor janitor, 
            IAspectExtractor aspectExtractor, 
            IAssetsCache cache, 
            IInjectionCollector injectionCollector, 
            IAspectWeaver aspectWeaver, 
            IEnumerable<IEffectWeaver> effectWeavers, 
            ILogger logger)
        {
            _aspectExtractor = aspectExtractor;
            _injectionCollector = injectionCollector;
            _cache = cache;
            _aspectWeaver = aspectWeaver;
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

        private void ProcessAssembly(AssemblyDefinition assembly)
        {
            var aspects = _aspectExtractor.Extract(assembly);

            foreach (var aspect in aspects)
                _cache.Cache(aspect);

            var injections = _injectionCollector.Collect(assembly).ToList();
            injections = ExcludeAspectInjections(injections, aspects);

            _janitor.Cleanup(assembly);

            if (_log.IsErrorThrown)
                return;

            _cache.FlushCache(assembly);

            foreach (var aspect in aspects)
                _aspectWeaver.WeaveGlobalAssests(aspect);

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

            var isInRelease = false;

            var debugAttr = assembly.CustomAttributes.FirstOrDefault(a => a.AttributeType.IsTypeOf(typeof(DebuggableAttribute)));
            if (!debugAttr.GetConstructorValue<DebuggableAttribute.DebuggingModes>(0).HasFlag(DebuggableAttribute.DebuggingModes.DisableOptimizations))
                isInRelease = true;

            foreach (var module in assembly.Modules)
            {
                if (isInRelease)
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