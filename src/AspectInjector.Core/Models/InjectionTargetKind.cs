namespace AspectInjector.Core.Models
{
    public enum InjectionTargetKind
    {
        AssemblyDefinition,
        ModuleDefinition,
        TypeDefinition,

        EventDefinition,
        PropertyDefinition,

        FieldDefinition,
        MethodDefinition,

        GenericParameter,
        MethodReturnType,
        ParameterDefinition
    }
}