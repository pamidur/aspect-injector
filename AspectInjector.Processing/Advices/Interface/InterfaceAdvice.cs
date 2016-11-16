using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System;

namespace AspectInjector.Core.Advices.Interface
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