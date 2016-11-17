using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System;

namespace AspectInjector.Core.MethodCall
{
    public class MethodCallAdvice : Advice, IEquatable<MethodCallAdvice>
    {
        public bool Equals(MethodCallAdvice other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(Advice other)
        {
            throw new NotImplementedException();
        }

        public bool IsApplicableFor(Aspect aspect)
        {
            throw new NotImplementedException();
        }

        protected override bool IsEqualTo(Advice other)
        {
            throw new NotImplementedException();
        }
    }
}