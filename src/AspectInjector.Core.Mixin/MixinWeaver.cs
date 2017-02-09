using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Fluent.Models;
using AspectInjector.Core.Services;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Mixin
{
    internal class MixinWeaver : EffectWeaverBase<TypeDefinition, MixinEffect>
    {
        public MixinWeaver(ILogger log) : base(log)
        {
            Priority = 10;
        }

        protected override void Weave(ResolvedInjection injection)
        {
            var ifaceTree = GetInterfacesTree(injection.Effect.InterfaceType);

            foreach (var iface in ifaceTree)
                if (injection.Target.Interfaces.All(i => !i.IsTypeOf(iface)))
                {
                    injection.Target.Interfaces.Add(injection.TypeSystem.Import(iface));

                    var ifaceDefinition = iface.Resolve();

                    foreach (var method in ifaceDefinition.Methods.Where(m => m.IsNormalMethod() || m.IsConstructor))
                        CreateMethodProxy(injection, method, iface);

                    foreach (var @event in ifaceDefinition.Events)
                        CreateEventProxy(injection, @event, iface);

                    foreach (var property in ifaceDefinition.Properties)
                        CreatePropertyProxy(injection, property, iface);
                }
        }

        protected MethodDefinition GetOrCreateMethodProxy(ResolvedInjection injection, MethodReference ifaceMethod)
        {
            var methodName = $"{ifaceMethod.DeclaringType.FullName}.{ifaceMethod.Name}";

            var proxy = injection.Target.Methods.FirstOrDefault(m => m.IsExplicitImplementationOf(ifaceMethod));
            if (proxy == null)
            {
                var callingMethod = ifaceMethod.ParametrizeGenericChild(ifaceMethod);

                var returnValue = injection.TypeSystem.Import(callingMethod.SafeReturnType());
                proxy = new MethodDefinition(methodName,
                    MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                     returnValue);

                injection.Target.Methods.Add(proxy);

                foreach (var gp in ifaceMethod.GenericParameters)
                    proxy.GenericParameters.Add((GenericParameter)injection.TypeSystem.Import(gp));

                if (ifaceMethod.Resolve().IsSpecialName)
                    proxy.IsSpecialName = true;

                proxy.Overrides.Add(injection.TypeSystem.Import(ifaceMethod));

                foreach (var parameter in callingMethod.Parameters)
                    proxy.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, injection.TypeSystem.Import(callingMethod.ResolveGenericType(parameter.ParameterType))));

                proxy.GetEditor().Instead(
                        e => e
                        .LoadAspect(injection.Source)
                        .Call(callingMethod, args => proxy.Parameters.ToList().ForEach(p => args.Load(p)))
                        .Return()
                    );
            }

            return proxy;
        }

        protected void CreateMethodProxy(ResolvedInjection injection, MethodDefinition originalMethod, TypeReference @interface)
        {
            var method = injection.TypeSystem.Import(originalMethod);

            if (@interface.IsGenericInstance)
                method = originalMethod.MakeHostInstanceGeneric((IGenericInstance)@interface);

            GetOrCreateMethodProxy(injection, method);
        }

        protected void CreateEventProxy(ResolvedInjection injection, EventDefinition originalEvent, TypeReference @interface)
        {
            var eventName = $"{@interface.FullName}.{originalEvent.Name}";
            var eventType = @interface.ResolveGenericType(originalEvent.EventType);

            var ed = injection.Target.Events.FirstOrDefault(e => e.Name == eventName && e.EventType.IsTypeOf(eventType));
            if (ed == null)
            {
                ed = new EventDefinition(eventName, EventAttributes.None, injection.TypeSystem.Import(eventType));

                if (originalEvent.AddMethod != null)
                {
                    MethodReference method = originalEvent.AddMethod;
                    if (@interface.IsGenericInstance) method = method.MakeHostInstanceGeneric((IGenericInstance)@interface);

                    ed.AddMethod = GetOrCreateMethodProxy(injection, method);
                }

                if (originalEvent.RemoveMethod != null)
                {
                    MethodReference method = originalEvent.RemoveMethod;
                    if (@interface.IsGenericInstance) method = method.MakeHostInstanceGeneric((IGenericInstance)@interface);

                    ed.RemoveMethod = GetOrCreateMethodProxy(injection, method);
                }

                injection.Target.Events.Add(ed);
            }
        }

        protected void CreatePropertyProxy(ResolvedInjection injection, PropertyDefinition originalProperty, TypeReference @interface)
        {
            var propertyName = $"{@interface.FullName}.{originalProperty.Name}";
            var propertyType = @interface.ResolveGenericType(originalProperty.PropertyType);

            var pd = injection.Target.Properties.FirstOrDefault(p => p.Name == propertyName && p.PropertyType.IsTypeOf(propertyType));
            if (pd == null)
            {
                pd = new PropertyDefinition(propertyName, PropertyAttributes.None, injection.TypeSystem.Import(propertyType));

                if (originalProperty.GetMethod != null)
                {
                    MethodReference method = originalProperty.GetMethod;
                    if (@interface.IsGenericInstance) method = method.MakeHostInstanceGeneric((IGenericInstance)@interface);

                    pd.GetMethod = GetOrCreateMethodProxy(injection, method);
                }

                if (originalProperty.SetMethod != null)
                {
                    MethodReference method = originalProperty.SetMethod;
                    if (@interface.IsGenericInstance) method = method.MakeHostInstanceGeneric((IGenericInstance)@interface);

                    pd.SetMethod = GetOrCreateMethodProxy(injection, method);
                }

                injection.Target.Properties.Add(pd);
            }
        }

        private static IEnumerable<TypeReference> GetInterfacesTree(TypeReference typeReference)
        {
            var definition = typeReference.Resolve();
            if (!definition.IsInterface)
                throw new NotSupportedException(typeReference.Name + " should be an interface");

            var nestedIfaces = definition.Interfaces.Select(typeReference.ParametrizeGenericChild);

            return new[] { typeReference }.Concat(nestedIfaces);
        }
    }
}