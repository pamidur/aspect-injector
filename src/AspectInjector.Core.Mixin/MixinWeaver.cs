using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Fluent.Models;
using AspectInjector.Core.Models;
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

        protected override void Weave(TypeDefinition target, MixinEffect mixin, Injection injection)
        {
            var ts = target.Module.GetTypeSystem();

            var ifaceTree = GetInterfacesTree(mixin.InterfaceType);

            foreach (var iface in ifaceTree)
                if (target.Interfaces.All(i => !i.IsTypeOf(iface)))
                {
                    target.Interfaces.Add(ts.Import(iface));

                    var ifaceDefinition = iface.Resolve();

                    foreach (var method in ifaceDefinition.Methods.Where(m => m.IsNormalMethod() || m.IsConstructor))
                        CreateMethodProxy(target, method, iface, injection, ts);

                    foreach (var @event in ifaceDefinition.Events)
                        CreateEventProxy(target, @event, iface, injection, ts);

                    foreach (var property in ifaceDefinition.Properties)
                        CreatePropertyProxy(target, property, iface, injection, ts);
                }
        }

        protected MethodDefinition GetOrCreateMethodProxy(TypeDefinition target, MethodReference ifaceMethod, Injection injection, ExtendedTypeSystem ts)
        {
            var methodName = $"{ifaceMethod.DeclaringType.FullName}.{ifaceMethod.Name}";

            var proxy = target.Methods.FirstOrDefault(m => m.IsExplicitImplementationOf(ifaceMethod));
            if (proxy == null)
            {
                var callingMethod = ifaceMethod.ParametrizeGenericChild(ifaceMethod);

                var returnValue = ts.Import(callingMethod.SafeReturnType());
                proxy = new MethodDefinition(methodName,
                    MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                     returnValue);

                target.Methods.Add(proxy);

                foreach (var gp in ifaceMethod.GenericParameters)
                    proxy.GenericParameters.Add((GenericParameter)ts.Import(gp));

                if (ifaceMethod.Resolve().IsSpecialName)
                    proxy.IsSpecialName = true;

                proxy.Overrides.Add(ts.Import(ifaceMethod));

                foreach (var parameter in callingMethod.Parameters)
                    proxy.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, ts.Import(callingMethod.ResolveGenericType(parameter.ParameterType))));

                proxy.GetEditor().Instead(
                        e => e
                        .LoadAspect(injection.Source)
                        .Call(callingMethod, args => proxy.Parameters.ToList().ForEach(p => args.Load(p)))
                        .Return()
                    );
            }

            return proxy;
        }

        protected void CreateMethodProxy(TypeDefinition target, MethodDefinition originalMethod, TypeReference @interface, Injection injection, ExtendedTypeSystem ts)
        {
            var method = ts.Import(originalMethod);

            if (@interface.IsGenericInstance)
                method = originalMethod.MakeHostInstanceGeneric((IGenericInstance)@interface);

            GetOrCreateMethodProxy(target, method, injection, ts);
        }

        protected void CreateEventProxy(TypeDefinition target, EventDefinition originalEvent, TypeReference @interface, Injection injection, ExtendedTypeSystem ts)
        {
            var eventName = $"{@interface.FullName}.{originalEvent.Name}";
            var eventType = @interface.ResolveGenericType(originalEvent.EventType);

            var ed = target.Events.FirstOrDefault(e => e.Name == eventName && e.EventType.IsTypeOf(eventType));
            if (ed == null)
            {
                ed = new EventDefinition(eventName, EventAttributes.None, ts.Import(eventType));

                if (originalEvent.AddMethod != null)
                {
                    MethodReference method = originalEvent.AddMethod;
                    if (@interface.IsGenericInstance) method = method.MakeHostInstanceGeneric((IGenericInstance)@interface);

                    ed.AddMethod = GetOrCreateMethodProxy(target, method, injection, ts);
                }

                if (originalEvent.RemoveMethod != null)
                {
                    MethodReference method = originalEvent.RemoveMethod;
                    if (@interface.IsGenericInstance) method = method.MakeHostInstanceGeneric((IGenericInstance)@interface);

                    ed.RemoveMethod = GetOrCreateMethodProxy(target, method, injection, ts);
                }

                target.Events.Add(ed);
            }
        }

        protected void CreatePropertyProxy(TypeDefinition target, PropertyDefinition originalProperty, TypeReference @interface, Injection injection, ExtendedTypeSystem ts)
        {
            var propertyName = $"{@interface.FullName}.{originalProperty.Name}";
            var propertyType = @interface.ResolveGenericType(originalProperty.PropertyType);

            var pd = target.Properties.FirstOrDefault(p => p.Name == propertyName && p.PropertyType.IsTypeOf(propertyType));
            if (pd == null)
            {
                pd = new PropertyDefinition(propertyName, PropertyAttributes.None, ts.Import(propertyType));

                if (originalProperty.GetMethod != null)
                {
                    MethodReference method = originalProperty.GetMethod;
                    if (@interface.IsGenericInstance) method = method.MakeHostInstanceGeneric((IGenericInstance)@interface);

                    pd.GetMethod = GetOrCreateMethodProxy(target, method, injection, ts);
                }

                if (originalProperty.SetMethod != null)
                {
                    MethodReference method = originalProperty.SetMethod;
                    if (@interface.IsGenericInstance) method = method.MakeHostInstanceGeneric((IGenericInstance)@interface);

                    pd.SetMethod = GetOrCreateMethodProxy(target, method, injection, ts);
                }

                target.Properties.Add(pd);
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