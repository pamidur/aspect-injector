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

        public static readonly TypeReference DebuggerHiddenAttribute = StandardTypes.GetType(typeof(DebuggerHiddenAttribute));
        public static readonly TypeReference CompilerGeneratedAttribute = StandardTypes.GetType(typeof(CompilerGeneratedAttribute));
        public static readonly TypeReference DebuggerStepThroughAttribute = StandardTypes.GetType(typeof(DebuggerStepThroughAttribute));
        public static readonly TypeReference AttributeArray = new ArrayType(StandardTypes.Attribute);
        public static readonly TypeReference MethodBase = StandardTypes.GetType(typeof(System.Reflection.MethodBase));

        public static readonly TypeReference Func_ObjectArray_Object =
            StandardTypes.GetType(typeof(Func<,>))
            .MakeGenericInstanceType(StandardTypes.ObjectArray, StandardTypes.Object);
    }
}
