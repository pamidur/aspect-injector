namespace AspectInjector.Core.Models
{
    public enum InjectionTargetType
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