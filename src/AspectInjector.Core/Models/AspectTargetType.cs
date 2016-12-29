namespace AspectInjector.Core.Models
{
    public enum AspectTargetKind
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