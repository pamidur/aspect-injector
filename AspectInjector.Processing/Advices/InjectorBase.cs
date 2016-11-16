using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Advices
{
    public abstract class InjectorBase<T> : IInjector<T>
        where T : IAdvice
    {
        protected ILogger Log { get; private set; }

        public void Apply(Aspect aspect, T advice)
        {
            if (aspect is Aspect<AssemblyDefinition>)
                Apply((Aspect<AssemblyDefinition>)aspect, advice);
            else if (aspect is Aspect<ModuleDefinition>)
                Apply((Aspect<ModuleDefinition>)aspect, advice);
            else if (aspect is Aspect<TypeDefinition>)
                Apply((Aspect<TypeDefinition>)aspect, advice);
        }

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
        }
    }
}