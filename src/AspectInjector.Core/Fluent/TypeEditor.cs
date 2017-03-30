//using AspectInjector.Core.Contexts;
//using Mono.Cecil;
//using System;
//using System.Collections.Generic;
//using System.Linq.Expressions;

//namespace AspectInjector.Core.Fluent
//{
//    public class TypeEditor
//    {
//        internal TypeDefinition Definition { get; private set; }

//        internal TypeEditor(EditorContext ctx, TypeDefinition td)
//        {
//            _ctx = ctx;
//            Definition = td;
//        }

//        private List<FluentInterfaceImplementation> _ifaceImplementations = new List<FluentInterfaceImplementation>();
//        private readonly EditorContext _ctx;

//        public IEnumerable<MethodEditor> Methods { get; set; }

//        public TypeEditor ImplementInterface(Expression<Action<FluentInterfaceImplementation>> action)
//        {
//            return this;
//        }

//        //public T Create<T>(Func<FluentMemberConstructor, T> action)
//        //{
//        //    return action(new FluentMemberConstructor(_ctx, this));
//        //}

//        /// <summary>
//        /// top-down execution
//        /// assemblyConstructor.execute -> fills all module constructors
//        /// </summary>
//        internal void Execute()
//        {
//        }
//    }
//}