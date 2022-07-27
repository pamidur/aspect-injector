using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace AspectInjector
{
    internal static class Program
    {
        private static bool _headerPrinted = false;
        private static int Main(string[] args)
        {
            if (args.Length == 0)
                return ShowHelp();

            var optimize = false;
            var verbose = false;
            List<string> references = new List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                switch (arg)
                {
                    case "-h":
                        return ShowHelp();
                    case "-d":
                        AttachDebugger();
                        continue;
                    case "-o":
                        optimize = true;
                        continue;
                    case "-v":
                        verbose = true;
                        ShowVerboseHeader();
                        continue;
                    case "-rf":
                        var reflist = args[++i];
                        if (File.Exists(reflist))
                            references.AddRange(File.ReadAllLines(reflist, Encoding.UTF8));
                        else return ShowHelp();
                        continue;
                    default:
                        references.AddRange(new ArraySegment<string>(args, i + 1, args.Length - i - 1));
                        return new Compiler().Execute(arg, references, optimize, verbose);
                }
            }

            return ShowHelp();
        }

        private static void AttachDebugger()
        {
            Console.WriteLine("DEBUG MODE!!! Waiting 10 sec for debugger to attach!");
            Console.WriteLine($"Process id is '{Process.GetCurrentProcess().Id}'");
            Debugger.Launch();
            var c = 0;
            while (!Debugger.IsAttached && c < 20)
            {
                Thread.Sleep(1000);
                Console.Write(".");
                c++;
            }

            Console.WriteLine();

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Debugger attached.");
                Debugger.Break();
            }
            else
            {
                Console.WriteLine("Debugger is not attached.");
            }
        }

        private static void ShowVerboseHeader()
        {
            if (!_headerPrinted)
            {
                var assembly = Assembly.GetExecutingAssembly();
                Console.WriteLine($"version: {assembly.GetCustomAttribute<AssemblyProductAttribute>().Product} {assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}");
                Console.WriteLine($"framework: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
                Console.WriteLine($"runtime: {System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier}");
                Console.WriteLine($"visit: https://github.com/pamidur/aspect-injector");
                _headerPrinted = true;
            }
        }

        private static int ShowHelp()
        {
            ShowVerboseHeader();
            Console.WriteLine();
            Console.WriteLine($"usage: AspectInjector [-d] [-o] [-v] [-rf <references_file>] <path_to_assembly> (<path_to_reference>)");
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
