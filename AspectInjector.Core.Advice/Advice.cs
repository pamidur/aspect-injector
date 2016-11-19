using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System;

namespace AspectInjector.Core.MethodCall
{
    public class Advice : Injection, IEquatable<Advice>
    {
        public bool Equals(Advice other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(Injection other)
        {
            throw new NotImplementedException();
        }

        public bool IsApplicableFor(Aspect aspect)
        {
            throw new NotImplementedException();
        }

        protected override bool IsEqualTo(Injection other)
        {
            throw new NotImplementedException();
        }
    }
}