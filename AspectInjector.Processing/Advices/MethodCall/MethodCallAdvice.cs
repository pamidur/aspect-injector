using AspectInjector.Contracts;
using AspectInjector.Models;
using Mono.Cecil;
using System;

namespace AspectInjector.Advices.MethodCall
{
    public class MethodCallAdvice : IAdvice
    {
        public TypeReference HostType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsApplicableFor(Aspect aspect)
        {
            throw new NotImplementedException();
        }
    }
}