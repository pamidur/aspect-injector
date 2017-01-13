using Mono.Cecil;

namespace AspectInjector.Core.Services.Extraction
{
    public interface Janitor
    {
        void Visit(AssemblyDefinition assembly);

        void Visit(ModuleDefinition module);

        void Visit(TypeDefinition type);

        void Visit(MethodDefinition method);

        void Visit(PropertyDefinition property);

        void Visit(EventDefinition @event);

        void Visit(FieldDefinition field);

        void Visit(ParameterDefinition param);

        void Visit(GenericParameter @event);

        void Visit(MethodReturnType @event);

        void Visit(CustomAttribute @event);
    }
}