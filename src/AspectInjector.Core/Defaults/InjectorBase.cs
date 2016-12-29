using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System;

namespace AspectInjector.Core.Defaults
{
    public abstract class InjectorBase<T> : IInjector
        where T : Injection
    {
        protected ILogger Log { get; private set; }

        protected ProcessingContext Context { get; private set; }

        public byte Priority { get; protected set; }

        protected virtual void Apply(AspectUsage<AssemblyDefinition> aspect, T injection)
        {
        }

        protected virtual void Apply(AspectUsage<ModuleDefinition> aspect, T injection)
        {
        }

        protected virtual void Apply(AspectUsage<TypeDefinition> aspect, T injection)
        {
        }

        protected virtual void Apply(AspectUsage<MethodDefinition> aspect, T injection)
        {
        }

        public void Init(ProcessingContext context)
        {
            Log = context.Services.Log;
            Context = context;
        }

        public void Apply(AspectUsage aspect, Injection injection)
        {
            var typedInjection = (T)injection;

            if (aspect is AspectUsage<AssemblyDefinition>)
                Apply((AspectUsage<AssemblyDefinition>)aspect, typedInjection);
            else if (aspect is AspectUsage<ModuleDefinition>)
                Apply((AspectUsage<ModuleDefinition>)aspect, typedInjection);
            else if (aspect is AspectUsage<TypeDefinition>)
                Apply((AspectUsage<TypeDefinition>)aspect, typedInjection);
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