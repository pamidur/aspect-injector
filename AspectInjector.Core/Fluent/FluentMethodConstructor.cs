using AspectInjector.Core.Contexts;
using Mono.Cecil;
using System;

namespace AspectInjector.Core.Fluent
{
    public class FluentMethodConstructor
    {
        private readonly MethodDefinition _md;

        public FluentMethodConstructor(ProcessingContext context, MethodDefinition md)
        {
        }

        public FluentMethodConstructor AddVariable(Action<VariableContructor> action)
        {
            return this;
        }

        internal void Execute()
        {
        }
    }
}