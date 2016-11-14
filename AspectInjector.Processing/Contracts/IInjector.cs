using AspectInjector.Contexts;
using AspectInjector.Models;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Contracts
{
    public interface IInjector
    {
        void Init(ProcessingContext context);

        bool CanProcess(IAdvice advice);

        void Apply(Aspect aspect, IAdvice advice);
    }
}