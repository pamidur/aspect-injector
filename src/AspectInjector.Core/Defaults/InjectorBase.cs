using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System;

namespace AspectInjector.Core.Defaults
{
    public abstract class InjectorBase<T> : IInjector
        where T : Effect
    {
        protected ILogger Log { get; private set; }

        protected ProcessingContext Context { get; private set; }

        public byte Priority { get; protected set; }

        protected virtual void Apply(Injection<AssemblyDefinition> aspect, T injection)
        {
        }

        protected virtual void Apply(Injection<ModuleDefinition> aspect, T injection)
        {
        }

        protected virtual void Apply(Injection<TypeDefinition> aspect, T injection)
        {
        }

        protected virtual void Apply(Injection<MethodDefinition> aspect, T injection)
        {
        }

        public void Init(ProcessingContext context)
        {
            Log = context.Services.Log;
            Context = context;
        }

        public void Apply(Injection aspect, Effect injection)
        {
            var typedInjection = (T)injection;

            if (aspect is Injection<AssemblyDefinition>)
                Apply((Injection<AssemblyDefinition>)aspect, typedInjection);
            else if (aspect is Injection<ModuleDefinition>)
                Apply((Injection<ModuleDefinition>)aspect, typedInjection);
            else if (aspect is Injection<TypeDefinition>)
                Apply((Injection<TypeDefinition>)aspect, typedInjection);
        }

        public bool CanApply(Effect injection)
        {
            return injection is T && CanApply((T)injection);
        }

        protected virtual bool CanApply(T injection)
        {
            return true;
        }
    }
}