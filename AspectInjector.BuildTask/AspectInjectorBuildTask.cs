using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using System;
using System.IO;

namespace AspectInjector.BuildTask
{
    public class AspectInjectorBuildTask : Task
    {
        [Required]
        public string Assembly { get; set; }

        [Required]
        public string OutputPath { get; set; }

        [Required]
        public string[] References { get; set; }

        public override bool Execute()
        {
            try
            {
#if DEBUG
                System.Diagnostics.Debugger.Launch();
#endif

                Console.WriteLine("Aspect Injector has started for {0}", Assembly);

                var assemblyResolver = new StrictAssemblyResolver();

                foreach (var r in References)
                    assemblyResolver.RegisterAssembly(r);

                string assemblyFile = Path.Combine(OutputPath, Assembly);
                string pdbFile = Path.Combine(OutputPath, Path.GetFileNameWithoutExtension(Assembly) + ".pdb");

                var assembly = AssemblyDefinition.ReadAssembly(assemblyFile,
                    new ReaderParameters
                    {
                        ReadingMode = Mono.Cecil.ReadingMode.Deferred,
                        AssemblyResolver = assemblyResolver,
                        ReadSymbols = true
                    });

                Console.WriteLine("Assembly has been loaded");

                var injector = new AspectInjector();
                injector.Process(assembly);

                Console.WriteLine("Assembly has been patched");

                assembly.Write(assemblyFile, new WriterParameters()
                {
                    WriteSymbols = true
                });

                Console.WriteLine("Assembly has been written");
            }
            catch (Exception e)
            {
                this.Log.LogErrorFromException(e, false, true, null);
                Console.Error.WriteLine(e.Message);
                return false;
            }

            return true;
        }
    }
}