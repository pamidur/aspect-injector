using AspectInjector.Contracts;
using AspectInjector.Models;
using Mono.Cecil;
using System;

namespace AspectInjector.Advices.Interface
{
    public class InterfaceAdvice : IAdvice
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