using AspectInjector.Core;
using AspectInjector.Core.Advice;
using AspectInjector.Core.Advice.Weavers;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Mixin;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Services;
using AspectInjector.Core.Utils;
using AspectInjector.Rules;
using DryIoc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AspectInjector.Commands
{
    public class ProcessCommand : ICommand
    {
        public string Description => "Instructs Aspect Injector to process injections.";

        public int Execute(IReadOnlyList<string> args)
        {
            if (args.Count == 0)
            {
                ShowHelp();
                return -1;
            }

            var filename = args[0];

            var optimize = args[1] == "-o";

            var refsStart = optimize ? 2 : 1;

            var references = new ArraySegment<string>(args.ToArray(), refsStart, args.Count - refsStart);

            if (!File.Exists(filename))
            {
                ShowHelp($"File {filename} does not exist.");
                return 1;
            }

            var log = new Logger();
            var processor = CreateProcessor(log);

            var resolver = new KnownReferencesAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(filename));
            references.ToList().ForEach(resolver.AddReference);

            try
            {
                processor.Process(filename, resolver, optimize);
            }
            catch (Exception e)
            {
                log.Log(GeneralRules.CompilationMustSecceedIfNoOtherErrors, e.ToString());
            }

            return log.IsErrorThrown ? 1 : 0;
        }
        private Processor CreateProcessor(ILogger log)
        {
            var container = new Container();

            //register main services

            container.Register<Processor>(Reuse.Singleton);
            container.Register<IAspectReader, AspectReader>(Reuse.Singleton);
            container.Register<IAspectWeaver, AspectWeaver>(Reuse.Singleton);
            container.Register<IInjectionReader, InjectionReader>(Reuse.Singleton);
            container.UseInstance(log, true);

            //register effects

            container.Register<IEffectReader, MixinReader>();
            container.Register<IEffectReader, AdviceReader>();

            container.Register<IEffectWeaver, MixinWeaver>();
            container.Register<IEffectWeaver, AdviceInlineWeaver>();
            container.Register<IEffectWeaver, AdviceAroundWeaver>();
            container.Register<IEffectWeaver, AdviceStateMachineWeaver>();

            //done registration

            return container.Resolve<Processor>();
        }

        private void ShowHelp(params string[] errors)
        {
            Program.ShowHeader();
            Console.WriteLine("PROCESS USAGE:");
            Console.WriteLine();
            Console.WriteLine(" AspectInjector.exe process <assembly> [-o] [<references>]");

            if (errors.Any())
            {
                Console.WriteLine();
                Console.WriteLine("ERRORS:");
                foreach (var error in errors)
                    Console.WriteLine($" {error}");
            }
            Console.WriteLine();
        }
    }
}