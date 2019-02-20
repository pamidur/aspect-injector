using AspectInjector.Broker;
using FluentIL;
using FluentIL.Extensions;
using Mono.Cecil;
using System;
using System.Diagnostics;

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
        public static readonly TypeReference AttributeArray = new ArrayType(StandardTypes.Attribute);
        public static readonly TypeReference MethodBase = StandardTypes.GetType(typeof(System.Reflection.MethodBase));

        public static readonly TypeReference Func_ObjectArray_Object =
            StandardTypes.GetType(typeof(Func<,>))
            .MakeGenericInstanceReference(StandardTypes.ObjectArray, StandardTypes.Object);
    }
}
