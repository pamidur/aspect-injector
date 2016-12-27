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
    internal class MixinInjector : InjectorBase<Mixin>
    {
        public MixinInjector()
        {
            Priority = 10;
        }

        protected override void Apply(Aspect<TypeDefinition> aspect, Mixin mixin)
        {
            var ts = Context.Editors.GetContext(aspect.Target.Module).TypeSystem;

            var ifaceTree = GetInterfacesTree(mixin.InterfaceType);

            foreach (var iface in ifaceTree)
                if (aspect.Target.Interfaces.All(i => !i.IsTypeOf(iface)))
                {
                    aspect.Target.Interfaces.Add(ts.Import(iface));

                    var gargs = new TypeReference[] { };

                    if (iface.IsGenericInstance)
                        gargs = ((GenericInstanceType)iface).GenericArguments.Select(ga => ts.Import(ga)).ToArray();

                    var ifaceDefinition = iface.Resolve();

                    foreach (var method in ifaceDefinition.Methods.Where(m => !m.IsAddOn && !m.IsRemoveOn && !m.IsSetter && !m.IsGetter))
                    {
                        MethodReference m = method;
                        if (gargs.Any()) m = m.MakeHostInstanceGeneric(gargs);
                        GetOrCreateMethodProxy(m, aspect, ts);
                    }

                    foreach (var @event in ifaceDefinition.Events)
                        CreateEventProxy(@event, gargs, aspect, ts);

                    foreach (var property in ifaceDefinition.Properties)
                        CreatePropertyProxy(property, gargs, aspect, ts);
                }
        }

        protected MethodDefinition GetOrCreateMethodProxy(MethodReference method, Aspect<TypeDefinition> aspect, ExtendedTypeSystem ts)
        {
            var targetType = aspect.Target;

            var methodName = $"{method.DeclaringType.FullName}.{method.Name}";

            var proxy = targetType.Methods.FirstOrDefault(m => m.IsExplicitImplementationOf(method));
            if (proxy == null)
            {
                proxy = new MethodDefinition(methodName,
                    MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                     ts.Import(method.SafeReturnType(), method));

                targetType.Methods.Add(proxy);

                if (method.Resolve().IsSpecialName)
                    proxy.IsSpecialName = true;

                foreach (var gp in method.GenericParameters)
                    //proxy.GenericParameters.Add(new GenericParameter(gp.Name, proxy));
                    proxy.GenericParameters.Add(gp);

                proxy.Overrides.Add(ts.Import(method));

                foreach (var parameter in method.Parameters)
                    proxy.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, ts.Import(method.ResolveGenericType(parameter.ParameterType), proxy)));

                var me = Context.Editors.GetEditor(proxy);

                me.Instead(e =>
                    e.Return(ret =>
                    ret.Load(aspect).Call(method, args => proxy.Parameters.ToList().ForEach(p => args.Load(p)))));
            }

            return proxy;
        }

        protected void CreateEventProxy(EventDefinition originalEvent, TypeReference[] genericArgs, Aspect<TypeDefinition> aspect, ExtendedTypeSystem ts)
        {
            TypeReference basetype = originalEvent.DeclaringType;

            if (genericArgs.Any())
                basetype = basetype.MakeGenericInstanceType(genericArgs);

            var eventName = $"{basetype.FullName}.{originalEvent.Name}";

            var eventType = basetype.ResolveGenericType(originalEvent.EventType);

            var ed = aspect.Target.Events.FirstOrDefault(e => e.Name == eventName && e.EventType.IsTypeOf(eventType));
            if (ed == null)
            {
                ed = new EventDefinition(eventName, EventAttributes.None, ts.Import(eventType));

                if (originalEvent.AddMethod != null)
                {
                    MethodReference method = originalEvent.AddMethod;
                    if (genericArgs.Any()) method = method.MakeHostInstanceGeneric(genericArgs);

                    ed.AddMethod = GetOrCreateMethodProxy(method, aspect, ts);
                }

                if (originalEvent.RemoveMethod != null)
                {
                    MethodReference method = originalEvent.RemoveMethod;
                    if (genericArgs.Any()) method = method.MakeHostInstanceGeneric(genericArgs);

                    ed.RemoveMethod = GetOrCreateMethodProxy(method, aspect, ts);
                }

                aspect.Target.Events.Add(ed);
            }
        }

        protected void CreatePropertyProxy(PropertyDefinition originalProperty, TypeReference[] genericArgs, Aspect<TypeDefinition> aspect, ExtendedTypeSystem ts)
        {
            TypeReference basetype = originalProperty.DeclaringType;

            if (genericArgs.Any())
                basetype = basetype.MakeGenericInstanceType(genericArgs);

            var propertyName = $"{basetype.FullName}.{originalProperty.Name}";

            var propertyType = basetype.ResolveGenericType(originalProperty.PropertyType);

            var pd = aspect.Target.Properties.FirstOrDefault(p => p.Name == propertyName && p.PropertyType.IsTypeOf(propertyType));
            if (pd == null)
            {
                pd = new PropertyDefinition(propertyName, PropertyAttributes.None, ts.Import(propertyType));

                if (originalProperty.GetMethod != null)
                {
                    MethodReference method = originalProperty.GetMethod;
                    if (genericArgs.Any()) method = method.MakeHostInstanceGeneric(genericArgs);

                    pd.GetMethod = GetOrCreateMethodProxy(method, aspect, ts);
                }

                if (originalProperty.SetMethod != null)
                {
                    MethodReference method = originalProperty.SetMethod;
                    if (genericArgs.Any()) method = method.MakeHostInstanceGeneric(genericArgs);

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