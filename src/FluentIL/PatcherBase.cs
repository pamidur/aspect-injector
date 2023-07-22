using dnlib.DotNet;
using dnlib.DotNet.Writer;
using FluentIL.Resolvers;
using System.Collections.Generic;
using System.IO;

namespace FluentIL;

public abstract class PatcherBase
{
    protected PatcherBase() { }

    public void Process(string assemblyFile, IReadOnlyList<string> references, bool optimize, bool verbose)
    {
        var resolver = GetResolver(assemblyFile, references);
        if (IsErrorThrown()) return;

        Process(assemblyFile, resolver, optimize, verbose);
    }

    public void Process(string assemblyFile, IAssemblyResolver resolver, bool optimize, bool verbose)
    {
        if (!File.Exists(assemblyFile))
        {
            OnError($"Target not found: '{assemblyFile}'");
            return;
        }

        if (verbose) OnInfo($"Started for {Path.GetFileName(assemblyFile)}");

        var pdbPresent = AreSymbolsFound(assemblyFile);
        var assembly = ReadAssembly(assemblyFile, resolver, pdbPresent, verbose);

        var modified = PatchAssembly(assembly, optimize, verbose);

        if (!IsErrorThrown())
        {
            if (modified)
            {
                if (verbose) OnInfo("Assembly has been patched.");
                WriteAssembly(assembly, assemblyFile, pdbPresent, verbose);
            }
            else if (verbose) OnInfo("No patching required.");
        }
    }

    protected virtual IAssemblyResolver GetResolver(string assemblyFile, IReadOnlyList<string> references)
    {
        var resolver = new KnownReferencesAssemblyResolver();

        if (!File.Exists(assemblyFile)) OnError($"Target not found: '{assemblyFile}'");
        else resolver.AddSearchDirectory(Path.GetDirectoryName(assemblyFile));

        foreach (var refr in references)
        {
            if (!File.Exists(refr)) OnError($"Reference not found: '{refr}'");
            else resolver.AddReference(refr);
        }

        return resolver;
    }

    private AssemblyDef ReadAssembly(string assemblyFile, IAssemblyResolver resolver, bool readSymbols, bool verbose)
    {

        ModuleContext modCtx = ModuleDef.CreateModuleContext();
        modCtx.AssemblyResolver = resolver;

        var assembly = AssemblyDef.Load(assemblyFile, new ModuleCreationOptions()
        {
            Context = modCtx,
            TryToLoadPdbFromDisk = readSymbols
        });

        if (verbose) OnInfo("Assembly has been read.");

        return assembly;
    }

    private void WriteAssembly(AssemblyDef assembly, string fileName, bool writeSymbols, bool verbose)
    {
        var param = new ModuleWriterOptions(assembly.ManifestModule);

        if (writeSymbols)
        {
            param.WritePdb = true;
        }

        assembly.Write(fileName, param);

        if (verbose) OnInfo("Assembly has been written.");
    }

    private bool AreSymbolsFound(string dllPath)
    {
        var pdbPath = Path.Combine(Path.GetDirectoryName(dllPath), Path.GetFileNameWithoutExtension(dllPath) + ".pdb");

        if (File.Exists(pdbPath))
        {
            return true;
        }

        OnInfo($"Symbols not found on {pdbPath}. Proceeding without...");
        return false;
    }

    protected virtual void OnInfo(string message) { }
    protected virtual void OnError(string message) { }
    protected abstract bool IsErrorThrown();
    protected abstract bool PatchAssembly(AssemblyDef assembly, bool optimize, bool verbose);
}