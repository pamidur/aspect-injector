using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Defaults
{
    public abstract class InjectorBase<T> : IInjector
        where T : Injection
    {
        protected ILogger Log { get; private set; }

        protected ProcessingContext Context { get; private set; }

        protected virtual void Apply(Aspect<AssemblyDefinition> aspect, T injection)
        {
        }

        protected virtual void Apply(Aspect<ModuleDefinition> aspect, T injection)
        {
        }

        protected virtual void Apply(Aspect<TypeDefinition> aspect, T injection)
        {
        }

        protected virtual void Apply(Aspect<MethodDefinition> aspect, T injection)
        {
        }

        public void Init(ProcessingContext context)
        {
            Log = context.Services.Log;
            Context = context;
        }

        public void Apply(Aspect aspect, Injection injection)
        {
            var typedInjection = (T)injection;

            if (aspect is Aspect<AssemblyDefinition>)
                Apply((Aspect<AssemblyDefinition>)aspect, typedInjection);
            else if (aspect is Aspect<ModuleDefinition>)
                Apply((Aspect<ModuleDefinition>)aspect, typedInjection);
            else if (aspect is Aspect<TypeDefinition>)
                Apply((Aspect<TypeDefinition>)aspect, typedInjection);
        }

        public bool CanApply(Injection injection)
        {
            return injection is T && CanApply((T)injection);
        }

        protected virtual bool CanApply(T injection)
        {
            return true;
        }
    }
}