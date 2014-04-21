using System;
using System.IO;
using System.Runtime.InteropServices;
using AspectInjector.BuildTask.Common;
using AspectInjector.BuildTask.Processors;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;

namespace AspectInjector.BuildTask
{
    [ComVisible(false)]
    public class AspectInjectorBuildTask : Task
    {
        [Required]
        public string Assembly { get; set; }

        [Required]
        public string OutputPath { get; set; }

        //public string StrongKeyPath { get; set; }
        //public bool Sing { get; set; }
        //public bool DelaySing { get; set; }

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

                var injector = new AssemblyProcessor(Configuration.GetProcessorsTree());
                injector.Process(assembly);

                Console.WriteLine("Assembly has been patched");

                assembly.Write(assemblyFile, new WriterParameters()
                {
                    WriteSymbols = true,
                    //StrongNameKeyPair = Sing && !DelaySing ? new StrongNameKeyPair(StrongKeyPath) : null
                });

                Console.WriteLine("Assembly has been written");
            }
            catch (CompilationException ce)
            {
                this.Log.LogError("Compilation exception", null, null,
                    ce.SequencePoint.Document.Url,
                    ce.SequencePoint.StartLine,
                    ce.SequencePoint.StartColumn,
                    ce.SequencePoint.EndLine,
                    ce.SequencePoint.EndColumn,
                    ce.Message);
                Console.Error.WriteLine(ce.Message);
                return false;
            }
            catch (Exception e)
            {
                this.Log.LogErrorFromException(e, true, true, null);
                Console.Error.WriteLine(e.Message);
                return false;
            }

            return true;
        }
    }
}