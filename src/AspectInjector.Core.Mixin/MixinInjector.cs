using AspectInjector.Core.Defaults;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent.Models;
using AspectInjector.Core.Models;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Mixin
{
    internal class MixinInjector : WeaverBase<Mixin>
    {
        public MixinInjector()
        {
            Priority = 10;
        }

        protected override void Apply(Injection<TypeDefinition> injection, Mixin mixin)
        {
            var ts = Context.Editors.GetContext(injection.Target.Module).TypeSystem;

            var ifaceTree = GetInterfacesTree(mixin.InterfaceType);

            foreach (var iface in ifaceTree)
                if (injection.Target.Interfaces.All(i => !i.IsTypeOf(iface)))
                {
                    injection.Target.Interfaces.Add(ts.Import(iface));

                    var ifaceDefinition = iface.Resolve();

                    foreach (var method in ifaceDefinition.Methods.Where(m => !m.IsAddOn && !m.IsRemoveOn && !m.IsSetter && !m.IsGetter))
                        CreateMethodProxy(method, iface, injection, ts);

                    foreach (var @event in ifaceDefinition.Events)
                        CreateEventProxy(@event, iface, injection, ts);

                    foreach (var property in ifaceDefinition.Properties)
                        CreatePropertyProxy(property, iface, injection, ts);
                }
        }

        protected MethodDefinition GetOrCreateMethodProxy(MethodReference ifaceMethod, Injection<TypeDefinition> aspect, ExtendedTypeSystem ts)
        {
            var targetType = aspect.Target;

            var methodName = $"{ifaceMethod.DeclaringType.FullName}.{ifaceMethod.Name}";

            var proxy = targetType.Methods.FirstOrDefault(m => m.IsExplicitImplementationOf(ifaceMethod));
            if (proxy == null)
            {
                var callingMethod = ifaceMethod.ParametrizeGenericChild(ifaceMethod);

                var returnValue = ts.Import(callingMethod.SafeReturnType());

                proxy = new MethodDefinition(methodName,
                    MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                     returnValue);

                targetType.Methods.Add(proxy);

                foreach (var gp in ifaceMethod.GenericParameters)
                    proxy.GenericParameters.Add((GenericParameter)ts.Import(gp));

                if (ifaceMethod.Resolve().IsSpecialName)
                    proxy.IsSpecialName = true;

                proxy.Overrides.Add(ts.Import(ifaceMethod));

                foreach (var parameter in callingMethod.Parameters)
                    proxy.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, ts.Import(callingMethod.ResolveGenericType(parameter.ParameterType))));

                Context.Editors.GetEditor(proxy).Instead(
                        e =>
                        e.Return(ret =>
                        ret.Load(aspect).Call(callingMethod, args => proxy.Parameters.ToList().ForEach(p => args.Load(p))))
                    );
            }

            return proxy;
        }

        protected void CreateMethodProxy(MethodDefinition originalMethod, TypeReference @interface, Injection<TypeDefinition> aspect, ExtendedTypeSystem ts)
        {
            var method = ts.Import(originalMethod);

            if (@interface.IsGenericInstance)
                method = originalMethod.MakeHostInstanceGeneric((IGenericInstance)@interface);

            GetOrCreateMethodProxy(method, aspect, ts);
        }

        protected void CreateEventProxy(EventDefinition originalEvent, TypeReference @interface, Injection<TypeDefinition> aspect, ExtendedTypeSystem ts)
        {
            var eventName = $"{@interface.FullName}.{originalEvent.Name}";
            var eventType = @interface.ResolveGenericType(originalEvent.EventType);

            var ed = aspect.Target.Events.FirstOrDefault(e => e.Name == eventName && e.EventType.IsTypeOf(eventType));
            if (ed == null)
            {
                ed = new EventDefinition(eventName, EventAttributes.None, ts.Import(eventType));

                if (originalEvent.AddMethod != null)
                {
                    MethodReference method = originalEvent.AddMethod;
                    if (@interface.IsGenericInstance) method = method.MakeHostInstanceGeneric((IGenericInstance)@interface);

                    ed.AddMethod = GetOrCreateMethodProxy(method, aspect, ts);
                }

                if (originalEvent.RemoveMethod != null)
                {
                    MethodReference method = originalEvent.RemoveMethod;
                    if (@interface.IsGenericInstance) method = method.MakeHostInstanceGeneric((IGenericInstance)@interface);

                    ed.RemoveMethod = GetOrCreateMethodProxy(method, aspect, ts);
                }

                aspect.Target.Events.Add(ed);
            }
        }

        protected void CreatePropertyProxy(PropertyDefinition originalProperty, TypeReference @interface, Injection<TypeDefinition> aspect, ExtendedTypeSystem ts)
        {
            var propertyName = $"{@interface.FullName}.{originalProperty.Name}";
            var propertyType = @interface.ResolveGenericType(originalProperty.PropertyType);

            var pd = aspect.Target.Properties.FirstOrDefault(p => p.Name == propertyName && p.PropertyType.IsTypeOf(propertyType));
            if (pd == null)
            {
                pd = new PropertyDefinition(propertyName, PropertyAttributes.None, ts.Import(propertyType));

                if (originalProperty.GetMethod != null)
                {
                    MethodReference method = originalProperty.GetMethod;
                    if (@interface.IsGenericInstance) method = method.MakeHostInstanceGeneric((IGenericInstance)@interface);

                    pd.GetMethod = GetOrCreateMethodProxy(method, aspect, ts);
                }

                if (originalProperty.SetMethod != null)
                {
                    MethodReference method = originalProperty.SetMethod;
                    if (@interface.IsGenericInstance) method = method.MakeHostInstanceGeneric((IGenericInstance)@interface);

                    pd.SetMethod = GetOrCreateMethodProxy(method, aspect, ts);
                }

                aspect.Target.Properties.Add(pd);
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