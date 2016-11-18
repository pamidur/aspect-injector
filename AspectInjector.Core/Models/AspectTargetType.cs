namespace AspectInjector.Core.Models
{
    public enum AspectTargetType
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