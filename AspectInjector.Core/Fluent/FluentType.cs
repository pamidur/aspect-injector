using AspectInjector.Core.Contexts;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AspectInjector.Core.Fluent
{
    public class FluentType
    {
        internal TypeDefinition Definition { get; private set; }

        internal FluentType(FluentContext ctx, TypeDefinition td)
        {
            _ctx = ctx;
            Definition = td;
        }

        private List<FluentInterfaceImplementation> _ifaceImplementations = new List<FluentInterfaceImplementation>();
        private readonly FluentContext _ctx;

        public IEnumerable<FluentMethod> Methods { get; set; }

        public FluentType ImplementInterface(Expression<Action<FluentInterfaceImplementation>> action)
        {
            return this;
        }

        public T Create<T>(Func<FluentMemberConstructor, T> action)
        {
            return action(new FluentMemberConstructor(_ctx, this));
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