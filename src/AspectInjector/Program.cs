using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace AspectInjector
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length == 0)
                return ShowHelp();

            var optimize = false;
            var verbose = false;
            string target = null;
            List<string> references = new List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                switch (arg)
                {
                    case "-h":
                        return ShowHelp();
                    case "-d":
                        Debugger.Launch();
                        continue;
                    case "-o":
                        optimize = true;
                        continue;
                    case "-v":
                        verbose = true;
                        continue;
                    case "-rf":
                        var reflist = args[++i];
                        if (File.Exists(reflist))
                            references.AddRange(File.ReadAllLines(reflist, Encoding.UTF8));
                        else return ShowHelp();
                        continue;
                    default:
                        target = arg;
                        references.AddRange(new ArraySegment<string>(args, i + 1, args.Length - i - 1));
                        return new Compiler().Execute(target, references, optimize, verbose);
                }
            }

            return ShowHelp();
        }

        private static int ShowHelp()
        {
            var assembly = Assembly.GetExecutingAssembly();
            Console.WriteLine($"version: {assembly.GetCustomAttribute<AssemblyProductAttribute>().Product} {assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}");
            Console.WriteLine($"visit: https://github.com/pamidur/aspect-injector");
            Console.WriteLine();
            Console.WriteLine($"usage: dotnet {assembly.ManifestModule.Name} [-d] [-o] [-v] [-rf <references_file>] <path_to_assembly> (<path_to_reference>)");
            Console.WriteLine($"options:");
            Console.WriteLine($"   -d\tAttach debugger.");
            Console.WriteLine($"   -o\tOptimize modified code.");
            Console.WriteLine($"   -v\tVerbose log.");
            Console.WriteLine($"   -rf\tPath to file with list of references. New line separated.");
            Console.WriteLine();

            return -1;
        }
    }
}