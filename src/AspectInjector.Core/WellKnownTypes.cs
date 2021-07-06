using AspectInjector.Broker;
using FluentIL;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AspectInjector.Core
{
    public static class WellKnownTypes
    {
        public static readonly string Injection = typeof(Injection).FullName;
        public static readonly string Aspect = typeof(Aspect).FullName;
        public static readonly string Mixin = typeof(Mixin).FullName;
        public static readonly string Advice = typeof(Advice).FullName;
        public static readonly string Argument = typeof(Argument).FullName;
        public static readonly string SkipInjection = typeof(SkipInjection).FullName;

        public static readonly StandardType Attribute_Array = new StandardType("System", "Attribute", isArray: true);
        public static readonly StandardType Object_Array = new StandardType("System", "Object", isArray: true);
        public static readonly StandardType Object = new StandardType("System", "Object");

        public static readonly StandardType CompilerGeneratedAttribute = new StandardType("System.Runtime.CompilerServices", "CompilerGeneratedAttribute");
        public static readonly StandardType DebuggerHiddenAttribute = new StandardType("System.Diagnostics", "DebuggerHiddenAttribute", assemblyHints: new[] { "System.Diagnostics.Debug" });
        public static readonly StandardType DebuggerStepThroughAttribute = new StandardType("System.Diagnostics", "DebuggerStepThroughAttribute", assemblyHints: new[] { "System.Diagnostics.Debug" });
        
        public static readonly StandardType Func_ObjectArray_Object = new StandardType("System", "Func`2", elements: new[] { Object_Array, Object });           
    }
}
