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

        public override bool Execute()
        {
            try
            {
                Console.WriteLine("Aspect Injector has started for {0}", Assembly);

                var assemblyResolver = new DefaultAssemblyResolver();
                assemblyResolver.AddSearchDirectory(OutputPath);

                string assemblyFile = Path.Combine(OutputPath, Assembly + ".exe");

                var assembly = AssemblyDefinition.ReadAssembly(assemblyFile,
                    new ReaderParameters
                    {
                        ReadingMode = Mono.Cecil.ReadingMode.Deferred,
                        AssemblyResolver = assemblyResolver
                    });

                Console.WriteLine("Assembly has been loaded");

                var injector = new AspectInjector();
                injector.Process(assembly);

                Console.WriteLine("Assembly has been patched");

                assembly.Write(assemblyFile);

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