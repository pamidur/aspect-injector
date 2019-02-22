using System;
using System.Diagnostics;
using System.Reflection;

namespace AspectInjector
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length == 0)
                return ShowHelp();

            var optimize = false;
            string target = null;
            ArraySegment<string> references = null;

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
                    default:
                        target = arg;
                        references = new ArraySegment<string>(args, i + 1, args.Length - i - 1);
                        return new Compiler().Execute(target, references, optimize);
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
            Console.WriteLine($"usage: dotnet {assembly.ManifestModule.Name} [-d] [-o] <path_to_assembly> (<path_to_reference>)");
            Console.WriteLine($"options:");
            Console.WriteLine($"   -d\tAttach debugger.");
            Console.WriteLine($"   -o\tOptimize modified code.");
            Console.WriteLine();

            return -1;
        }
    }
}