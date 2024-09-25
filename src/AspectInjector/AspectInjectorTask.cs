using System.Diagnostics;
using System.Text;
using Microsoft.Build.Framework;

namespace AspectInjector;

public class AspectInjectorTask : Microsoft.Build.Utilities.Task
{    
    [Required]
    public required string AssemblyPath { get; init; }
    public ITaskItem[] References { get; set; } = [];

    public bool AttachDebugger { get; init; }
    public bool Optimize { get; init; }
    public bool Verbose { get; init; }   
    
    public override bool Execute()
    {
        if (AttachDebugger) AttachDebuggerNow();
        return new Compiler().Execute(AssemblyPath, References.Select(r=>r.GetMetadata("FullPath")).ToArray(), Optimize, Verbose, Log);
    }

    private static void AttachDebuggerNow()
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
}
