using AspectInjector.Contexts;
using Mono.Cecil;
using System.IO;

namespace AspectInjector
{
    public class Processor
    {
        private readonly Configuration _config;

        public Processor(Configuration config)
        {
            _config = config;
        }

        public void Process(string assemblyFile, string[] references)
        {
            _config.Log.LogMessage($"Aspect Injector has started for {Path.GetFileName(assemblyFile)}");

            var context = new ProcessingContext
            {
                Log = _config.Log,
                Assembly = ReadAssembly(assemblyFile, references),
            };

            context.Assembly.MainModule.Resources.Add(new EmbeddedResource())

            foreach (var processor in _processors)
            {
                processor.ProcessModule(assembly.MainModule);
            }

            _config.Log.LogMessage("Assembly has been patched");
        }

        private AssemblyDefinition ReadAssembly(string assemblyFile, string[] references)
        {
            var resolver = new StrictAssemblyResolver(references);

            var assembly = AssemblyDefinition.ReadAssembly(assemblyFile,
                new ReaderParameters
                {
                    ReadingMode = ReadingMode.Deferred,
                    AssemblyResolver = resolver,
                    ReadSymbols = true
                });

            _config.Log.LogMessage("Assembly has been loaded");

            return assembly;
        }

        private void WriteAssembly(AssemblyDefinition assembly, string path)
        {
            assembly.Write(path,
                new WriterParameters()
                {
                    WriteSymbols = true,
                    ////StrongNameKeyPair = Sing && !DelaySing ? new StrongNameKeyPair(StrongKeyPath) : null
                });

            _config.Log.LogMessage("Assembly has been written");
        }
    }
}