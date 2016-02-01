using AspectInjector.BuildTask.Contexts;
using AspectInjector.BuildTask.Contracts;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using System.Linq;

namespace AspectInjector.BuildTask.Injectors
{
    internal class InterfaceInjector : IAspectInjector<InterfaceInjectionContext>
    {
        public void Inject(InterfaceInjectionContext context)
        {
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
                var newAddMethod = GetOrCreateMethodProxy(context, originalEvent.AddMethod);
                var newRemoveMethod = GetOrCreateMethodProxy(context, originalEvent.RemoveMethod);

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
                var newGetMethod = GetOrCreateMethodProxy(context, originalProperty.GetMethod);
                var newSetMethod = GetOrCreateMethodProxy(context, originalProperty.SetMethod);

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
                ctx.EntryPoint.LoadFieldOntoStack(aspectField);
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