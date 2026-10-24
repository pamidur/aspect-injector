using Mono.Cecil;
using System;

namespace FluentIL.Resolvers
{
    /// <summary>
    /// Resolves self-references without transferring target ownership to the dependency cache.
    /// </summary>
    internal sealed class TargetAssemblyResolver : IAssemblyResolver
    {
        private readonly IAssemblyResolver _dependencies;
        private AssemblyDefinition _target;
        private IDisposable _registration;

        /// <summary>
        /// Wraps a dependency resolver without taking ownership of it.
        /// </summary>
        public TargetAssemblyResolver(IAssemblyResolver dependencies)
        {
            _dependencies = dependencies;
        }

        /// <summary>
        /// Gets or sets the operation-owned target and its scoped dependency registration.
        /// </summary>
        public AssemblyDefinition Target
        {
            get => _target;
            set
            {
                _registration?.Dispose();
                _registration = null;
                _target = value;
                // Make dependency back-references find the active target too.
                if (value != null && _dependencies is CachedAssemblyResolver resolver)
                    _registration = resolver.UseTarget(value);
            }
        }

        /// <summary>
        /// Returns the active target for self-references and delegates dependency lookups.
        /// </summary>
        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            if (Target != null && name.FullName == Target.Name.FullName)
                return Target;
            return _dependencies.Resolve(name);
        }

        /// <summary>
        /// Resolves self-references directly and forwards reader settings for dependencies.
        /// </summary>
        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            if (Target != null && name.FullName == Target.Name.FullName)
                return Target;

            return _dependencies.Resolve(name, parameters);
        }

        /// <summary>
        /// Ends the target registration without disposing the target or dependency resolver.
        /// </summary>
        public void Dispose()
        {
            Target = null;
        }
    }
}
