using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace FluentIL.Resolvers
{
    public class CachedAssemblyResolver : BaseAssemblyResolver
    {
        private readonly Dictionary<string, AssemblyDefinition> _cache = new Dictionary<string, AssemblyDefinition>();
        // Active targets are borrowed; only persistent cache entries are owned here.
        private readonly Dictionary<string, AssemblyDefinition> _targets = new Dictionary<string, AssemblyDefinition>();

        /// <summary>
        /// Resolves active targets before consulting the owned dependency cache.
        /// </summary>
        public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            // Preloaded dependencies resolve back-references through this instance too.
            if (_targets.TryGetValue(name.FullName, out var target))
                return target;

            var result = _cache.ContainsKey(name.FullName) ? _cache[name.FullName] : null;

            if (result == null)
            {
                result = LookupAssembly(name, parameters);
                _cache[name.FullName] = result;
            }

            return result;
        }

        /// <summary>
        /// Temporarily registers an operation-owned target for dependency back-references.
        /// </summary>
        internal IDisposable UseTarget(AssemblyDefinition assembly) => new TargetScope(this, assembly);

        private sealed class TargetScope : IDisposable
        {
            private CachedAssemblyResolver _resolver;
            private readonly string _name;
            private readonly AssemblyDefinition _previous;

            /// <summary>
            /// Registers the target while saving any enclosing target registration.
            /// </summary>
            public TargetScope(CachedAssemblyResolver resolver, AssemblyDefinition assembly)
            {
                _resolver = resolver;
                _name = assembly.Name.FullName;
                resolver._targets.TryGetValue(_name, out _previous);
                resolver._targets[_name] = assembly;
            }

            /// <summary>
            /// Restores the enclosing target registration without disposing the target.
            /// </summary>
            public void Dispose()
            {
                if (_resolver == null) return;
                // Nested processing must restore the enclosing target registration.
                if (_previous == null)
                    _resolver._targets.Remove(_name);
                else
                    _resolver._targets[_name] = _previous;
                _resolver = null;
            }
        }

        protected virtual AssemblyDefinition LookupAssembly(AssemblyNameReference name, ReaderParameters parameters)
        {
            return base.Resolve(name, parameters);
        }

        protected internal void RegisterAssembly(AssemblyDefinition assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            var name = assembly.Name.FullName;
            if (_cache.ContainsKey(name))
                return;

            _cache[name] = assembly;
        }

        /// <summary>
        /// Disposes cached dependency assemblies without disposing borrowed targets.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Deduplicate entries and make repeated disposal harmless.
                var assemblies = new HashSet<AssemblyDefinition>(_cache.Values);
                _cache.Clear();
                foreach (var assembly in assemblies)
                    assembly.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
