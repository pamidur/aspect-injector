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
                    var ifaceRef = _ts.Import(iface);

                    _target.Interfaces.Add(ifaceRef);

                    var ifaceDefinition = iface.Resolve();

                    foreach (var method in ifaceDefinition.Methods.Where(m => m.IsNormalMethod() || m.IsConstructor))
                        CreateMethodProxy(method, ifaceRef);

                    foreach (var @event in ifaceDefinition.Events)
                        CreateEventProxy(@event, ifaceRef);

                    foreach (var property in ifaceDefinition.Properties)
                        CreatePropertyProxy(property, ifaceRef);
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
                     _ts.Import(ifaceMethod.ReturnType));

                _target.Methods.Add(proxy);

                foreach (var gp in ifaceMethod.GenericParameters)
                    proxy.GenericParameters.Add(new GenericParameter(gp.Name, proxy));

                proxy.ReturnType = _ts.Import(ifaceMethod.ResolveIfGeneric(ifaceMethod.ReturnType));

                if (ifaceMethod.Resolve().IsSpecialName)
                    proxy.IsSpecialName = true;

                foreach (var parameter in ifaceMethod.Parameters)
                    proxy.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, _ts.Import(ifaceMethod.ResolveIfGeneric(parameter.ParameterType))));

                proxy.Overrides.Add(ifaceMethod);

                proxy.GetEditor().Instead(
                    e => e
                    .LoadAspect(_aspect)
                    .Call(ifaceMethod, args => proxy.Parameters.ToList().ForEach(p => args.Load(p)))
                    .Return()
                    );
            }

            return proxy;
        }

        protected void CreateMethodProxy(MethodDefinition originalMethod, TypeReference @interface)
        {
            var method = _ts.Import(originalMethod);

            if (@interface.IsGenericInstance)
                method = _ts.Import(originalMethod.MakeHostInstanceGeneric(@interface));

            GetOrCreateMethodProxy(method);
        }

        protected void CreateEventProxy(EventDefinition originalEvent, TypeReference @interface)
        {
            var eventName = $"{@interface.FullName}.{originalEvent.Name}";
            var eventType = @interface.ResolveIfGeneric(_ts.Import(originalEvent.AddMethod ?? originalEvent.RemoveMethod).Parameters[0].ParameterType);
                        
            var ed = _target.Events.FirstOrDefault(e => e.Name == eventName && e.EventType.IsTypeOf(eventType));
            if (ed == null)
            {
                ed = new EventDefinition(eventName, EventAttributes.None, _ts.Import(eventType));

                if (originalEvent.AddMethod != null)
                {
                    MethodReference method = _ts.Import(originalEvent.AddMethod);
                    if (@interface.IsGenericInstance) method = _ts.Import(originalEvent.AddMethod.MakeHostInstanceGeneric(@interface));

                    ed.AddMethod = GetOrCreateMethodProxy(method);
                }

                if (originalEvent.RemoveMethod != null)
                {
                    MethodReference method = _ts.Import(originalEvent.RemoveMethod);
                    if (@interface.IsGenericInstance) method = _ts.Import(originalEvent.RemoveMethod.MakeHostInstanceGeneric(@interface));

                    ed.RemoveMethod = GetOrCreateMethodProxy(method);
                }

                _target.Events.Add(ed);
            }
        }

        protected void CreatePropertyProxy(PropertyDefinition originalProperty, TypeReference @interface)
        {
            var propertyName = $"{@interface.FullName}.{originalProperty.Name}";

            TypeReference propertyType = null;
            MethodReference getMethod = null;
            MethodReference setMethod = null;

            if (originalProperty.GetMethod != null)
            {
                getMethod = originalProperty.GetMethod;
                if (@interface.IsGenericInstance)
                    getMethod = originalProperty.GetMethod.MakeHostInstanceGeneric(@interface);

                propertyType = getMethod.ResolveIfGeneric(getMethod.ReturnType);
            }

            if (originalProperty.SetMethod != null)
            {
                setMethod = originalProperty.SetMethod;
                if (@interface.IsGenericInstance)
                    setMethod = originalProperty.SetMethod.MakeHostInstanceGeneric(@interface);

                propertyType = propertyType ?? setMethod.ResolveIfGeneric(setMethod.Parameters[0].ParameterType);
            }

            propertyType = _ts.Import(propertyType);
            if (getMethod != null) getMethod = _ts.Import(getMethod);
            if (setMethod != null) setMethod = _ts.Import(setMethod);

            var pd = _target.Properties.FirstOrDefault(p => p.Name == propertyName && p.PropertyType.IsTypeOf(propertyType));
            if (pd == null)
            {
                pd = new PropertyDefinition(propertyName, PropertyAttributes.None, propertyType);

                if (getMethod != null)
                    pd.GetMethod = GetOrCreateMethodProxy(getMethod);


                if (setMethod != null)
                    pd.SetMethod = GetOrCreateMethodProxy(setMethod);

                _target.Properties.Add(pd);
            }
        }

        private IEnumerable<TypeReference> GetInterfacesTree(TypeReference typeReference)
        {
            var definition = typeReference.Resolve();
            if (!definition.IsInterface)
                throw new NotSupportedException(typeReference.Name + " should be an interface");

            var nestedIfaces = definition.Interfaces.Select(i => typeReference.IsGenericInstance ? (typeReference).MakeCallReference(i) : i);

            return new[] { typeReference }.Concat(nestedIfaces);
        }
    }
}