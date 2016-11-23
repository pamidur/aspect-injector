using AspectInjector.Core.Defaults;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Mixin
{
    internal class MixinInjector : InjectorBase<Mixin>
    {
        protected override void Apply(Aspect<TypeDefinition> aspect, Mixin mixin)
        {
            var target = aspect.Target;

            new Fluent.FluentTypeConstructor(aspect.Target)
                .ImplementInterface(
                    ic =>
                    {
                        ic.ImplementMethod(
                            mc => mc.Equals()
                            );
                    }
                );

            if (!classDefinition.TypeDefinition.ImplementsInterface(interfaceDefinition))
            {
                var ifaces = interfaceDefinition.GetInterfacesTree();

                foreach (var iface in ifaces)
                    classDefinition.TypeDefinition.Interfaces.Add(classDefinition.TypeDefinition.Module.Import(iface));
            }
            else if (!classDefinition.TypeDefinition.Interfaces.Any(i => i.IsTypeOf(interfaceDefinition)))
            {
                //In order to behave the same as csc
                classDefinition.TypeDefinition.Interfaces.Add(classDefinition.TypeDefinition.Module.Import(interfaceDefinition));
            }

            context.Methods = interfaceDefinition.GetInterfaceTreeMembers(td => td.Methods)
                .Where(m => !m.IsAddOn && !m.IsRemoveOn && !m.IsSetter && !m.IsGetter)
                .ToArray();

            context.Properties = interfaceDefinition.GetInterfaceTreeMembers(td => td.Properties)
                .ToArray();

            context.Events = interfaceDefinition.GetInterfaceTreeMembers(td => td.Events)
                .ToArray();

            foreach (var method in context.Methods)
                GetOrCreateMethodProxy(context, method);

            foreach (var @event in context.Events)
                GetOrCreateEventProxy(context, @event);

            foreach (var property in context.Properties)
                GetOrCreatePropertyProxy(context, property);
        }

        protected EventDefinition GetOrCreateEventProxy(InterfaceInjectionContext context, EventDefinition originalEvent)
        {
            var eventName = GenerateMemberProxyName(originalEvent);

            var ed = context.AspectContext.TargetTypeContext.TypeDefinition.Events.FirstOrDefault(e => e.Name == eventName && e.EventType.IsTypeOf(originalEvent.EventType));
            if (ed == null)
            {
                var newAddMethod = originalEvent.AddMethod == null ? null : GetOrCreateMethodProxy(context, originalEvent.AddMethod);
                var newRemoveMethod = originalEvent.AddMethod == null ? null : GetOrCreateMethodProxy(context, originalEvent.RemoveMethod);

                ed = new EventDefinition(eventName, EventAttributes.None, context.AspectContext.TargetTypeContext.TypeDefinition.Module.Import(originalEvent.EventType));
                ed.AddMethod = newAddMethod;
                ed.RemoveMethod = newRemoveMethod;

                context.AspectContext.TargetTypeContext.TypeDefinition.Events.Add(ed);
            }

            return ed;
        }

        protected PropertyDefinition GetOrCreatePropertyProxy(InterfaceInjectionContext context, PropertyDefinition originalProperty)
        {
            var propertyName = GenerateMemberProxyName(originalProperty);

            var pd = context.AspectContext.TargetTypeContext.TypeDefinition.Properties.FirstOrDefault(p => p.Name == propertyName && p.PropertyType.IsTypeOf(originalProperty.PropertyType));
            if (pd == null)
            {
                var newGetMethod = originalProperty.GetMethod == null ? null : GetOrCreateMethodProxy(context, originalProperty.GetMethod);
                var newSetMethod = originalProperty.SetMethod == null ? null : GetOrCreateMethodProxy(context, originalProperty.SetMethod);

                pd = new PropertyDefinition(propertyName, PropertyAttributes.None, context.AspectContext.TargetTypeContext.TypeDefinition.Module.Import(originalProperty.PropertyType));
                pd.GetMethod = newGetMethod;
                pd.SetMethod = newSetMethod;

                context.AspectContext.TargetTypeContext.TypeDefinition.Properties.Add(pd);
            }

            return pd;
        }

        protected MethodDefinition GetOrCreateMethodProxy(InterfaceInjectionContext context, MethodDefinition interfaceMethodDefinition)
        {
            var targetType = context.AspectContext.TargetTypeContext.TypeDefinition;
            var methodName = GenerateMemberProxyName(interfaceMethodDefinition);

            var md = targetType.Methods.FirstOrDefault(m => m.Name == methodName && m.SignatureMatches(interfaceMethodDefinition));
            if (md == null)
            {
                {
                    FluentType type = null;
                    Aspect aspect = null;

                    type.CreateMethod(methodName, method =>
                    {
                        method.Attributes = MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
                        method.ReturnType = interfaceMethodDefinition.ReturnType;
                        method.IsSpecialName = interfaceMethodDefinition.IsSpecialName;

                        method.OnEntry(e =>
                        {
                            e.Return(ret => ret.Call(FluentMethod method, args =>)) CallMethod LoadAspect(aspect)
                        });
                    });
                }

                var ctx = context.AspectContext.TargetTypeContext.CreateMethod(methodName,
                    MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                    targetType.Module.Import(interfaceMethodDefinition.ReturnType));

                md = ctx.TargetMethod;

                if (interfaceMethodDefinition.IsSpecialName)
                    md.IsSpecialName = true;

                md.Overrides.Add(targetType.Module.Import(interfaceMethodDefinition));

                foreach (var parameter in interfaceMethodDefinition.Parameters)
                    md.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, md.Module.Import(parameter.ParameterType)));

                //TODO:: Use method from SnippetsProcessor
                foreach (var genericParameter in interfaceMethodDefinition.GenericParameters)
                    md.GenericParameters.Add(genericParameter);

                var aspectField = context.AspectContext.TargetTypeContext.GetOrCreateAspectReference(context.AspectContext);

                if (!aspectField.Resolve().IsStatic) ctx.EntryPoint.LoadSelfOntoStack();
                ctx.EntryPoint.LoadField(aspectField);
                ctx.EntryPoint.InjectMethodCall(interfaceMethodDefinition, md.Parameters.ToArray());
            }

            return md;
        }

        private static string GenerateMemberProxyName(IMemberDefinition member)
        {
            return member.DeclaringType.FullName + "." + member.Name;
        }
    }
}