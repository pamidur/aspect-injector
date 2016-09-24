namespace AspectInjector.BuildTask.Processors.ModuleProcessors
{
    internal enum FoundOn
    {
        /// <summary>
        ///     Applies to all members of all classes in the assembly.
        /// </summary>
        Assembly,

        /// <summary>
        ///     Applies to all members in the base and in all child types.
        /// </summary>
        BaseType,

        /// <summary>
        ///     Applies to the base member and to all overrides.
        /// </summary>
        BaseTypeMember,

        /// <summary>
        ///     Applies to all members implementing any of the interface members.
        /// </summary>
        Interface,

        /// <summary>
        ///     Applies to all implementing members of this interface member.
        /// </summary>
        InterfaceMember,

        /// <summary>
        ///     Applies to all members of the type.
        /// </summary>
        Type
    }
}