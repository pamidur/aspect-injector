using AspectInjector.Broker;
using System;

namespace AspectInjector.BuildTask.Validation
{
    internal class ValidationRule
    {
        public AdviceArgumentSource Source { get; set; }

        public Type ShouldBeOfType { get; set; }

        //public Type ShouldBeOfType { get; set; }
    }
}