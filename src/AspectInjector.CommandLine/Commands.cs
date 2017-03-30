using AspectInjector.Core;
using AspectInjector.Core.Configuration;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Mixin;
using AspectInjector.Core.Utils;
using CLAP;
using System.Diagnostics;
using System.IO;

namespace AspectInjector.CLI
{
    public class Commands
    {
        private ILogger Log { get; } = new ConsoleLogger();

        [Global(Aliases = "d", Description = "Launch a debugger")]
        public void Debug()
        {
            Debugger.Launch();
        }

        [Empty, Help]
        public void Help(string help)
        {
            Log.LogMessage(help);
        }

        [Error]
        private void HandleError(ExceptionContext context)
        {
            context.ReThrow = true;
        }

        [Verb(Aliases = "process", Description = "Instructs Aspect Injector to process injections")]
        public int Process(
            [Aliases("f")]
            [Required]
            [Description(".net assembly file for processing (typically exe or dll)")]
            string file)
        {
            if (!File.Exists(file))
            {
                Log.LogError($"File {file} does not exists.");
                return 1;
            }

            var processor = new Processor(
                ProcessingConfiguration.Default
                .SetLogger(Log)
                .UseMixinInjections()
                );

            var resolver = new CachedAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(file));

            processor.Process(file, resolver);

            return Log.IsErrorThrown ? 1 : 0;
        }
    }
}