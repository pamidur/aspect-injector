using AspectInjector.Core.Contexts;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AspectInjector.Core.Fluent
{
    public class FluentTypeConstructor
    {
        private readonly TypeDefinition _td;

        public FluentTypeConstructor(ProcessingContext context, TypeDefinition td)
        {
        }

        private List<FluentInterfaceImplementation> _ifaceImplementations = new List<FluentInterfaceImplementation>();

        public FluentTypeConstructor ImplementInterface(Expression<Action<FluentInterfaceImplementation>> action)
        {
            return this;
        }

        /// <summary>
        /// top-down execution
        /// assemblyConstructor.execute -> fills all module constructors
        /// </summary>
        internal void Execute()
        {
        }
    }
}