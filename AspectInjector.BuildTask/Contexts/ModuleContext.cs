using AspectInjector.BuildTask.Models;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.BuildTask.Contexts
{
    public class ModuleContext
    {
        #region Fields

        private static readonly Dictionary<ModuleDefinition, ModuleContext> Contexts = new Dictionary<ModuleDefinition, ModuleContext>();

        #endregion Fields

        #region Constructors

        private ModuleContext(ModuleDefinition module)
        {
            ModuleDefinition = module;
            TypeSystem = new ExtendedTypeSystem(module);
        }

        #endregion Constructors

        #region Properties

        public ModuleDefinition ModuleDefinition { get; private set; }

        public ExtendedTypeSystem TypeSystem { get; private set; }

        #endregion Properties

        #region Methods

        public static ModuleContext GetOrCreateContext(ModuleDefinition md)
        {
            lock (Contexts)
            {
                ModuleContext result;

                if (!Contexts.TryGetValue(md, out result))
                {
                    result = new ModuleContext(md);
                    Contexts.Add(md, result);
                }

                return result;
            }
        }

        #endregion Methods
    }
}