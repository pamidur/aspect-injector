using AspectInjector.BuildTask.Contracts;
using Mono.Cecil;

namespace AspectInjector.BuildTask.Processors.ModuleProcessors
{
    class SnippetsProcessor : IModuleProcessor
    {
        private static readonly string _namespace = "__$_aspect_injector_namespaces";

        public void ProcessModule(Mono.Cecil.ModuleDefinition module)
        {
            var td = new TypeDefinition(_namespace, "AspectFactory", TypeAttributes.Sealed | TypeAttributes.NotPublic | TypeAttributes.AnsiClass | TypeAttributes.AutoLayout | TypeAttributes.Abstract | TypeAttributes.BeforeFieldInit);
            td.BaseType = module.TypeSystem.Object;

            module.Types.Add(td);
        }
    }
}
