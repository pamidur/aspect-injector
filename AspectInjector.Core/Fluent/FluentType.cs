using AspectInjector.Core.Contexts;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AspectInjector.Core.Fluent
{
    public class FluentType
    {
        private readonly TypeDefinition _td;

        public FluentType(FluentContext context, TypeDefinition td)
        {
        }

        private List<FluentInterfaceImplementation> _ifaceImplementations = new List<FluentInterfaceImplementation>();

        public FluentType ImplementInterface(Expression<Action<FluentInterfaceImplementation>> action)
        {
            return this;
        }

        public FluentType CreateMethod(string name, Action<FluentMethod> action)
        {
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