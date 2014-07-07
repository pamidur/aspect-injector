using AspectInjector.BuildTask.Contracts;
using Mono.Cecil;
using System.IO;
using System.Linq;

namespace AspectInjector.BuildTask.Processors.ModuleProcessors
{
    class SnippetsProcessor : IModuleProcessor
    {
        public SnippetsProcessor()
        {            
            string assemblyFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),"AspectInjector.Snippets.dll");
            
            assembly = AssemblyDefinition.ReadAssembly(assemblyFile,
                new ReaderParameters
                {
                    ReadingMode = Mono.Cecil.ReadingMode.Deferred,
                });



        }

        private static readonly string _namespace = "__$_aspect_injector_namespaces";
        private AssemblyDefinition assembly;
        private ModuleDefinition module;

        public void ProcessModule(ModuleDefinition module)
        {
            this.module = module;

            foreach (var type in assembly.MainModule.Types.Where(t => t.Name != "<Module>"))
                MigrateType(module, type);
        }

        public void MigrateType(ModuleDefinition destination, TypeDefinition type)
        {
            var newtype = new TypeDefinition(_namespace, type.Name, type.Attributes, MigrateReference(module, type.BaseType));

            foreach (var field in type.Fields)
                MigrateField(destination, newtype, field);

            destination.Types.Add(newtype);
        }

        private void MigrateField(ModuleDefinition module, TypeDefinition newtype, FieldDefinition field)
        {
            var newField = new FieldDefinition(field.Name, field.Attributes, MigrateReference(module, field.FieldType));
            newtype.Fields.Add(newField);
        }

        private TypeReference MigrateReference(ModuleDefinition module, TypeReference reference)
        {
            return module.Import(reference);
        }
    }
}
