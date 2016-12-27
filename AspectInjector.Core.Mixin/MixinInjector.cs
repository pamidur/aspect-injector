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

            var target = aspect.Target;

            var ifaceTree = GetInterfacesTree(mixin.InterfaceType);

            foreach (var iface in ifaceTree)
                if (target.Interfaces.All(i => !i.IsTypeOf(iface)))
                {
                    target.Interfaces.Add(ts.Import(iface));

                    var gargs = new TypeReference[] { };

                    if (iface.IsGenericInstance)
                        gargs = ((GenericInstanceType)iface).GenericArguments.ToArray();

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
                     ts.Import(method.ResolveGenericType(method.ReturnType)));

                targetType.Methods.Add(proxy);

                if (method.Resolve().IsSpecialName)
                    proxy.IsSpecialName = true;

                proxy.Overrides.Add(ts.Import(method));

                foreach (var parameter in method.Parameters)
                    proxy.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, ts.Import(method.ResolveGenericType(parameter.ParameterType))));

                //TODO:: Use method from SnippetsProcessor, check generic args
                foreach (var genericParameter in method.GenericParameters)
                    proxy.GenericParameters.Add(genericParameter);

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

            var nestedIfaces = definition.Interfaces.ToList().AsEnumerable();

            nestedIfaces = nestedIfaces.Select(nested =>
            {
                if (nested.IsGenericInstance)
                {
                    var nestedGeneric = (GenericInstanceType)nested;

                    if (!nestedGeneric.ContainsGenericParameter)
                        return nestedGeneric;

                    var args = nestedGeneric.GenericArguments.Select(ga => typeReference.ResolveGenericType(ga)).ToArray();

                    return nested.Resolve().MakeGenericInstanceType(args);
                }
                else
                {
                    if (!nested.HasGenericParameters)
                        return nested;

                    var generic = (IGenericInstance)typeReference;

                    return nested.MakeGenericInstanceType(generic.GenericArguments.ToArray());
                }
            });

            return new[] { typeReference }.Concat(nestedIfaces);
        }
    }
}