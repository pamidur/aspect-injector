using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Fluent.Models;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Mixin
{
    internal class MixinWeaveProcess
    {
        private readonly TypeDefinition _target;
        private readonly MixinEffect _effect;
        private readonly ExtendedTypeSystem _ts;
        private readonly AspectDefinition _aspect;

        public MixinWeaveProcess(TypeDefinition target, AspectDefinition aspect, MixinEffect effect)
        {
            _target = target;
            _aspect = aspect;
            _effect = effect;
            _ts = target.Module.GetTypeSystem();
        }

        public void Execute()
        {
            var ifaceTree = GetInterfacesTree(_effect.InterfaceType);

            foreach (var iface in ifaceTree)
                if (_target.Interfaces.All(i => !i.IsTypeOf(iface)))
                {
                    _target.Interfaces.Add(_ts.Import(iface));

                    var ifaceDefinition = iface.Resolve();

                    foreach (var method in ifaceDefinition.Methods.Where(m => m.IsNormalMethod() || m.IsConstructor))
                        CreateMethodProxy(method, iface);

                    foreach (var @event in ifaceDefinition.Events)
                        CreateEventProxy(@event, iface);

                    foreach (var property in ifaceDefinition.Properties)
                        CreatePropertyProxy(property, iface);
                }
        }

        protected MethodDefinition GetOrCreateMethodProxy(MethodReference ifaceMethod)
        {
            var methodName = $"{ifaceMethod.DeclaringType.FullName}.{ifaceMethod.Name}";

            var proxy = _target.Methods.FirstOrDefault(m => m.IsExplicitImplementationOf(ifaceMethod));
            if (proxy == null)
            {
                proxy = new MethodDefinition(methodName,
                    MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                     _ts.Import(ifaceMethod.ResolveGenericType(ifaceMethod.ReturnType)));

                _target.Methods.Add(proxy);

                foreach (var gp in ifaceMethod.GenericParameters)
                    proxy.GenericParameters.Add(new GenericParameter(gp.Name, proxy));// (GenericParameter)_ts.Import(gp));

                if (ifaceMethod.Resolve().IsSpecialName)
                    proxy.IsSpecialName = true;

                proxy.Overrides.Add(_ts.Import(ifaceMethod));

                foreach (var parameter in ifaceMethod.Parameters)
                    proxy.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, _ts.Import(ifaceMethod.ResolveGenericType(parameter.ParameterType))));

                var callingMethod = proxy.ParametrizeGenericChild(ifaceMethod);

                proxy.GetEditor().Instead(
                        e => e
                        .LoadAspect(_aspect)
                        .Call(callingMethod, args => proxy.Parameters.ToList().ForEach(p => args.Load(p)))
                        .Return()
                    );
            }

            return proxy;
        }

        protected void CreateMethodProxy(MethodDefinition originalMethod, TypeReference @interface)
        {
            var method = _ts.Import(originalMethod);

            if (@interface.IsGenericInstance)
                method = originalMethod.MakeHostInstanceGeneric(@interface);

            GetOrCreateMethodProxy(method);
        }

        protected void CreateEventProxy(EventDefinition originalEvent, TypeReference @interface)
        {
            var eventName = $"{@interface.FullName}.{originalEvent.Name}";
            var eventType = @interface.ResolveGenericType(originalEvent.EventType);

            var ed = _target.Events.FirstOrDefault(e => e.Name == eventName && e.EventType.IsTypeOf(eventType));
            if (ed == null)
            {
                ed = new EventDefinition(eventName, EventAttributes.None, _ts.Import(eventType));

                if (originalEvent.AddMethod != null)
                {
                    MethodReference method = originalEvent.AddMethod;
                    if (@interface.IsGenericInstance) method = method.MakeHostInstanceGeneric(@interface);

                    ed.AddMethod = GetOrCreateMethodProxy(method);
                }

                if (originalEvent.RemoveMethod != null)
                {
                    MethodReference method = originalEvent.RemoveMethod;
                    if (@interface.IsGenericInstance) method = method.MakeHostInstanceGeneric(@interface);

                    ed.RemoveMethod = GetOrCreateMethodProxy(method);
                }

                _target.Events.Add(ed);
            }
        }

        protected void CreatePropertyProxy(PropertyDefinition originalProperty, TypeReference @interface)
        {
            var propertyName = $"{@interface.FullName}.{originalProperty.Name}";
            var propertyType = @interface.ResolveGenericType(originalProperty.PropertyType);

            var pd = _target.Properties.FirstOrDefault(p => p.Name == propertyName && p.PropertyType.IsTypeOf(propertyType));
            if (pd == null)
            {
                pd = new PropertyDefinition(propertyName, PropertyAttributes.None, _ts.Import(propertyType));

                if (originalProperty.GetMethod != null)
                {
                    MethodReference method = originalProperty.GetMethod;
                    if (@interface.IsGenericInstance) method = method.MakeHostInstanceGeneric(@interface);

                    pd.GetMethod = GetOrCreateMethodProxy(method);
                }

                if (originalProperty.SetMethod != null)
                {
                    MethodReference method = originalProperty.SetMethod;
                    if (@interface.IsGenericInstance) method = method.MakeHostInstanceGeneric(@interface);

                    pd.SetMethod = GetOrCreateMethodProxy(method);
                }

                _target.Properties.Add(pd);
            }
        }

        private static IEnumerable<TypeReference> GetInterfacesTree(TypeReference typeReference)
        {
            var definition = typeReference.Resolve();
            if (!definition.IsInterface)
                throw new NotSupportedException(typeReference.Name + " should be an interface");

            var nestedIfaces = definition.Interfaces.Select(i => typeReference.IsGenericInstance ? (typeReference).ParametrizeGenericChild(i) : i);

            return new[] { typeReference }.Concat(nestedIfaces);
        }
    }
}