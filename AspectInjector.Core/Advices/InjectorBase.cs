using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Advices
{
    public abstract class InjectorBase<T> : IInjector
        where T : IAdvice
    {
        protected ILogger Log { get; private set; }

        protected ProcessingContext Context { get; private set; }

        protected virtual void Apply(Aspect<AssemblyDefinition> aspect, T advice)
        {
        }

        protected virtual void Apply(Aspect<ModuleDefinition> aspect, T advice)
        {
        }

        protected virtual void Apply(Aspect<TypeDefinition> aspect, T advice)
        {
        }

        protected virtual void Apply(Aspect<MemberReference> aspect, T advice)
        {
        }

        public void Init(ProcessingContext context)
        {
            Log = context.Services.Log;
            Context = context;
        }

        public void Apply(Aspect aspect, IAdvice advice)
        {
            var typedAdvice = (T)advice;

            if (aspect is Aspect<AssemblyDefinition>)
                Apply((Aspect<AssemblyDefinition>)aspect, typedAdvice);
            else if (aspect is Aspect<ModuleDefinition>)
                Apply((Aspect<ModuleDefinition>)aspect, typedAdvice);
            else if (aspect is Aspect<TypeDefinition>)
                Apply((Aspect<TypeDefinition>)aspect, typedAdvice);
        }

        public bool CanApply(IAdvice advice)
        {
            return advice is T && CanApply((T)advice);
        }

        protected abstract bool CanApply(T advice);
    }
}