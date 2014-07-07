using AspectInjector.BuildTask.Contracts;
using Mono.Cecil;
using System.IO;

namespace AspectInjector.BuildTask.Processors.ModuleProcessors
{
    class SnippetsProcessor : IModuleProcessor
    {
        public SnippetsProcessor()
        {            
            string assemblyFile = System.Reflection.Assembly.GetExecutingAssembly().Location;
            
            var assembly = AssemblyDefinition.ReadAssembly(assemblyFile,
                new ReaderParameters
                {
                    ReadingMode = Mono.Cecil.ReadingMode.Deferred,
                });



        }

        private static readonly string _namespace = "__$_aspect_injector_namespaces";

        public void ProcessModule(Mono.Cecil.ModuleDefinition module)
        {
            var td = new TypeDefinition(_namespace, "AspectFactory", TypeAttributes.Sealed | TypeAttributes.NotPublic | TypeAttributes.AnsiClass | TypeAttributes.AutoLayout | TypeAttributes.Abstract | TypeAttributes.BeforeFieldInit);
            td.BaseType = module.TypeSystem.Object;

            module.Types.Add(td);
        }
    }
}
