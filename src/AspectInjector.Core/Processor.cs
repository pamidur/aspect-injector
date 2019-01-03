using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Models;
using AspectInjector.Rules;
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
            _log.Log(GeneralRules.Info, $"Started for {Path.GetFileName(assemblyFile)}");

            var pdbPresent = AreSymbolsFound(assemblyFile);
            var assembly = ReadAssembly(assemblyFile, resolver, pdbPresent);

            var modified = ProcessAssembly(assembly, optimize);

            if (!_log.IsErrorThrown)
            {
                if (modified)
                {
                    _log.Log(GeneralRules.Info, "Assembly has been patched.");
                    WriteAssembly(assembly, assemblyFile, pdbPresent);
                }
                else _log.Log(GeneralRules.Info, "No patching required.");
            }
        }


        private AssemblyDefinition ReadAssembly(string assemblyFile, IAssemblyResolver resolver, bool readSymbols)
        {
            var assembly = AssemblyDefinition.ReadAssembly(assemblyFile,
                new ReaderParameters
                {
                    ReadingMode = ReadingMode.Deferred
                });
            var name = assembly.Name;
            assembly.Dispose();

            assembly = resolver.Resolve(assembly.Name, new ReaderParameters
            {
                ReadingMode = ReadingMode.Deferred,
                ReadWrite = true,
                AssemblyResolver = resolver,
                ReadSymbols = readSymbols
            });

            _log.Log(GeneralRules.Info, "Assembly has been read.");

            return assembly;
        }

        private void WriteAssembly(AssemblyDefinition assembly, string path, bool writeSymbols)
        {
            var param = new WriterParameters();

            if (writeSymbols)
            {
                param.WriteSymbols = true;

                //if (assembly.MainModule.SymbolReader != null)
                //    param.SymbolWriterProvider = assembly.MainModule.SymbolReader.GetWriterProvider();
            }

            assembly.Write(param);

            if (assembly.MainModule.SymbolReader != null)
                assembly.MainModule.SymbolReader.Dispose();

            assembly.Dispose();
            assembly = null;

            _log.Log(GeneralRules.Info, "Assembly has been written.");
        }

        private bool AreSymbolsFound(string dllPath)
        {
            var pdbPath = Path.Combine(Path.GetDirectoryName(dllPath), Path.GetFileNameWithoutExtension(dllPath) + ".pdb");

            if (File.Exists(pdbPath))
            {
                return true;
            }

            _log.Log(GeneralRules.Info, $"Symbols not found on {pdbPath}. Proceeding without...");
            return false;
        }

        public bool ProcessAssembly(AssemblyDefinition assembly, bool optimize)
        {
            var aspects = _aspectExtractor.ReadAll(assembly);
            _log.Log(GeneralRules.Info, $"Found {aspects.Count} aspects");

            var injections = _injectionCollector.ReadAll(assembly).ToList();
            _log.Log(GeneralRules.Info, $"Found {injections.Count} injections");

            if (_log.IsErrorThrown)
                return false;

            var hasAspects = aspects.Count != 0;
            var hasInjections = injections.Count != 0;

            if (hasAspects)
            {
                _log.Log(GeneralRules.Info, "Processing aspects...");
                foreach (var aspect in aspects)
                    _aspectWeaver.WeaveGlobalAssests(aspect);
            }

            if (hasInjections)
            {
                _log.Log(GeneralRules.Info, "Processing injections...");

                foreach (var injector in _effectWeavers.OrderByDescending(i => i.Priority))
                {
                    _log.Log(GeneralRules.Info, $"Executing {injector.GetType().Name}...");

                    foreach (var prioritizedInjections in injections.GroupBy(i => i.Priority).OrderByDescending(a => a.Key).ToList())
                        foreach (var injection in prioritizedInjections.OrderByDescending(i => i.Effect.Priority))
                            if (injector.CanWeave(injection))
                            {
                                injector.Weave(injection);
                                injections.Remove(injection);
                            }
                }

                foreach (var injection in injections)
                    _log.Log(GeneralRules.UnexpectedCompilerBehaviour, injection.Target, $"Couldn't find weaver for => {injection.ToString()}");
            }

            if (hasAspects || hasInjections)
            {
                if (optimize)
                    _log.Log(GeneralRules.Info, "Cleanup and optimize...");
                else
                    _log.Log(GeneralRules.Info, "Cleanup...");

                foreach (var module in assembly.Modules)
                {
                    if (optimize)
                        EditorFactory.Optimize(module);

                    EditorFactory.CleanUp(module);
                }

                return true;
            }

            return false;
        }

        private List<InjectionDefinition> ExcludeAspectInjections(IEnumerable<InjectionDefinition> injections, IEnumerable<AspectDefinition> aspects)
        {
            var aspectTypes = new HashSet<TypeDefinition>(aspects.Select(a => a.Host));
            return injections.Where(i => !aspectTypes.Contains(i.Target.DeclaringType)).ToList();
        }
    }
}