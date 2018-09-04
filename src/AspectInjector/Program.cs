using AspectInjector.Commands;
using AspectInjector.GlobalSwitches;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectInjector
{
    internal class Program
    {
        private static readonly IReadOnlyDictionary<string, ISwitch> _globalSwitches = new Dictionary<string, ISwitch>
        {
            { "-d", new DebugSwitch() }
        };

        private static readonly IReadOnlyDictionary<string, ICommand> _commands = new Dictionary<string, ICommand>
        {
            { "process", new ProcessCommand() }
        };        

        private static int Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (_globalSwitches.TryGetValue(args[i], out var @switch))
                {
                    var res = @switch.Enable();
                    if (res != 0) return res;
                    continue;
                }

                if (_commands.TryGetValue(args[i], out var command))
                {
                    var commandArgs = new ArraySegment<string>(args, i + 1, args.Length - i - 1);
                    return command.Execute(commandArgs);
                }

                ShowHelp();
                return -1;
            }

            ShowHelp();
            return -1;
        }

        public static void ShowHeader()
        {
            var assembly = Assembly.GetExecutingAssembly();
            Console.WriteLine($"{assembly.GetCustomAttribute<AssemblyProductAttribute>().Product} {assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}");
            Console.WriteLine($"{assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright}");
            Console.WriteLine();
        }

        private static void ShowHelp()
        {
            ShowHeader();
            Console.WriteLine("USAGE:");
            Console.WriteLine();
            Console.WriteLine(" Switches:");
            foreach (var @switch in _globalSwitches)
                Console.WriteLine($"  {@switch.Key}\t\t{@switch.Value.Description}");
            Console.WriteLine(" Commands:");
            foreach (var command in _commands)
                Console.WriteLine($"  {command.Key}\t{command.Value.Description}");
            Console.WriteLine();
        }
    }
}