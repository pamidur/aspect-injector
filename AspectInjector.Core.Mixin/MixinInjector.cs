using AspectInjector.Core.Defaults;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Mixin
{
    internal class MixinInjector : InjectorBase<Mixin>
    {
        protected override void Apply(Aspect<TypeDefinition> aspect, Mixin mixin)
        {
            var target = aspect.Target;

            if (!target.Implements(mixin.InterfaceType))
            {
                var ifaces = GetInterfacesTree(mixin.InterfaceType);

                foreach (var iface in ifaces)
                    target.Interfaces.Add(target.Module.Import(iface));
            }
            else if (!target.Interfaces.Any(i => i.IsTypeOf(mixin.InterfaceType)))
            {
                //In order to behave the same as csc
                target.Interfaces.Add(target.Module.Import(mixin.InterfaceType));
            }

            var methods = GetInterfaceTreeMembers(mixin.InterfaceType, td => td.Methods)
                .Where(m => !m.IsAddOn && !m.IsRemoveOn && !m.IsSetter && !m.IsGetter)
                .ToArray();

            var props = GetInterfaceTreeMembers(mixin.InterfaceType, td => td.Properties)
                .ToArray();

            var events = GetInterfaceTreeMembers(mixin.InterfaceType, td => td.Events)
                .ToArray();

            foreach (var method in methods)
                GetOrCreateMethodProxy(method, aspect);

            foreach (var @event in events)
                GetOrCreateEventProxy(@event, aspect);

            foreach (var property in props)
                GetOrCreatePropertyProxy(property, aspect);
        }

        protected MethodDefinition GetOrCreateMethodProxy(MethodDefinition method, Aspect<TypeDefinition> aspect)
        {
            var targetType = aspect.Target;
            var methodName = GenerateMemberProxyName(method);

            var proxy = targetType.Methods.FirstOrDefault(m => m.Name == methodName && SignatureMatches(m, method));
            if (proxy == null)
            {
                proxy = new MethodDefinition(methodName,
                    MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                    targetType.Module.Import(method.ReturnType));

                targetType.Methods.Add(proxy);

                if (method.IsSpecialName)
                    proxy.IsSpecialName = true;

                proxy.Overrides.Add(targetType.Module.Import(method));

                foreach (var parameter in method.Parameters)
                    proxy.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, proxy.Module.Import(parameter.ParameterType)));

                //TODO:: Use method from SnippetsProcessor
                foreach (var genericParameter in method.GenericParameters)
                    proxy.GenericParameters.Add(genericParameter);

                var me = Context.Editors.GetEditor(proxy);

                me.Instead(e => e
                .Return(ret => ret.Load(aspect)
                .Call(method, args => proxy.Parameters.ToList().ForEach(p => args.Load(p)))));
            }

            return proxy;
        }

        protected EventDefinition GetOrCreateEventProxy(EventDefinition originalEvent, Aspect<TypeDefinition> aspect)
        {
            var eventName = GenerateMemberProxyName(originalEvent);

            var ed = aspect.Target.Events.FirstOrDefault(e => e.Name == eventName && e.EventType.IsTypeOf(originalEvent.EventType));
            if (ed == null)
            {
                var newAddMethod = originalEvent.AddMethod == null ? null : GetOrCreateMethodProxy(originalEvent.AddMethod, aspect);
                var newRemoveMethod = originalEvent.AddMethod == null ? null : GetOrCreateMethodProxy(originalEvent.RemoveMethod, aspect);

                ed = new EventDefinition(eventName, EventAttributes.None, aspect.Target.Module.Import(originalEvent.EventType));
                ed.AddMethod = newAddMethod;
                ed.RemoveMethod = newRemoveMethod;

                aspect.Target.Events.Add(ed);
            }

            return ed;
        }

        protected PropertyDefinition GetOrCreatePropertyProxy(PropertyDefinition originalProperty, Aspect<TypeDefinition> aspect)
        {
            var propertyName = GenerateMemberProxyName(originalProperty);

            var pd = aspect.Target.Properties.FirstOrDefault(p => p.Name == propertyName && p.PropertyType.IsTypeOf(originalProperty.PropertyType));
            if (pd == null)
            {
                var newGetMethod = originalProperty.GetMethod == null ? null : GetOrCreateMethodProxy(originalProperty.GetMethod, aspect);
                var newSetMethod = originalProperty.SetMethod == null ? null : GetOrCreateMethodProxy(originalProperty.SetMethod, aspect);

                pd = new PropertyDefinition(propertyName, PropertyAttributes.None, aspect.Target.Module.Import(originalProperty.PropertyType));
                pd.GetMethod = newGetMethod;
                pd.SetMethod = newSetMethod;

                aspect.Target.Properties.Add(pd);
            }

            return pd;
        }

        private static string GenerateMemberProxyName(IMemberDefinition member)
        {
            return member.DeclaringType.FullName + "." + member.Name;
        }

        private static IEnumerable<TypeReference> GetInterfacesTree(TypeReference typeReference)
        {
            var definition = typeReference.Resolve();
            if (!definition.IsInterface)
                throw new NotSupportedException(typeReference.Name + " should be an interface");

            return new[] { definition }.Concat(definition.Interfaces);
        }

        private static IEnumerable<T> GetInterfaceTreeMembers<T>(TypeReference typeReference, Func<TypeDefinition, IEnumerable<T>> selector)
        {
            var definition = typeReference.Resolve();
            if (!definition.IsInterface)
                throw new NotSupportedException(typeReference.Name + " should be an interface");

            var members = selector(definition);

            if (definition.Interfaces.Count > 0)
                members = members.Concat(definition.Interfaces.Select(i => selector(i.Resolve())).Aggregate((a, b) => a.Concat(b)));

            return members;
        }

        public static bool SignatureMatches(MethodReference methodReference1, MethodReference methodReference2)
        {
            if (methodReference1.IsGenericInstance && methodReference2.HasGenericParameters)
            {
                //todo:: check generic args and params
            }

            if (!methodReference1.MethodReturnType.ReturnType.IsTypeOf(methodReference2.MethodReturnType.ReturnType))
                return false;

            for (int i = 0; i < methodReference1.Parameters.Count; i++)
                if (!methodReference1.Parameters[i].ParameterType.IsTypeOf(methodReference2.Parameters[i].ParameterType))
                    return false;

            return true;
        }
    }
}