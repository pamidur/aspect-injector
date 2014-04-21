using System.Linq;
using AspectInjector.BuildTask.Contexts;
using AspectInjector.BuildTask.Contracts;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AspectInjector.BuildTask.Injectors
{
    class InterfaceInjector : InjectorBase, IAspectInjector<InterfaceInjectionContext>
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

            var ed = context.AspectContext.TargetType.Events.FirstOrDefault(e => e.Name == eventName && e.EventType.IsTypeOf(originalEvent.EventType));
            if (ed == null)
            {
                var newAddMethod = GetOrCreateMethodProxy(context, originalEvent.AddMethod);
                var newRemoveMethod = GetOrCreateMethodProxy(context, originalEvent.RemoveMethod);

                ed = new EventDefinition(eventName, EventAttributes.None, context.AspectContext.TargetType.Module.Import(originalEvent.EventType));
                ed.AddMethod = newAddMethod;
                ed.RemoveMethod = newRemoveMethod;

                context.AspectContext.TargetType.Events.Add(ed);
            }

            return ed;
        }

        protected PropertyDefinition GetOrCreatePropertyProxy(InterfaceInjectionContext context, PropertyDefinition originalProperty)
        {
            var propertyName = GenerateMemberProxyName(originalProperty);

            var pd = context.AspectContext.TargetType.Properties.FirstOrDefault(p => p.Name == propertyName && p.PropertyType.IsTypeOf(originalProperty.PropertyType));
            if (pd == null)
            {
                var newGetMethod = GetOrCreateMethodProxy(context, originalProperty.GetMethod);
                var newSetMethod = GetOrCreateMethodProxy(context, originalProperty.SetMethod);

                pd = new PropertyDefinition(propertyName, PropertyAttributes.None, context.AspectContext.TargetType.Module.Import(originalProperty.PropertyType));
                pd.GetMethod = newGetMethod;
                pd.SetMethod = newSetMethod;

                context.AspectContext.TargetType.Properties.Add(pd);
            }

            return pd;
        }

        private static string GenerateMemberProxyName(IMemberDefinition member)
        {
            return member.DeclaringType.FullName + "." + member.Name;
        }

        protected MethodDefinition GetOrCreateMethodProxy(InterfaceInjectionContext context, MethodDefinition interfaceMethodDefinition)
        {
            var targetType = context.AspectContext.TargetType;
            var methodName = GenerateMemberProxyName(interfaceMethodDefinition);

            var md = targetType.Methods.FirstOrDefault(m => m.Name == methodName && m.SignatureMatches(interfaceMethodDefinition));
            if (md == null)
            {
                md = new MethodDefinition(methodName,
                    MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                    targetType.Module.Import(interfaceMethodDefinition.ReturnType));

                if (interfaceMethodDefinition.IsSpecialName)
                    md.IsSpecialName = true;

                md.Overrides.Add(targetType.Module.Import(interfaceMethodDefinition));
                targetType.Methods.Add(md);

                foreach (var parameter in interfaceMethodDefinition.Parameters)
                    md.Parameters.Add(parameter);

                foreach (var genericParameter in interfaceMethodDefinition.GenericParameters)
                    md.GenericParameters.Add(genericParameter);

                var aspectField = GetOrCreateAspectReference(context.AspectContext);

                var processor = md.Body.GetILProcessor();
                processor.Append(processor.Create(OpCodes.Nop));

                var retCode = processor.Create(OpCodes.Ret);
                processor.Append(retCode);

                InjectMethodCall(processor, retCode, aspectField, interfaceMethodDefinition, md.Parameters.ToArray());

                if (!interfaceMethodDefinition.ReturnType.IsTypeOf(typeof(void)))
                {
                    md.Body.InitLocals = true;
                    md.Body.Variables.Add(new VariableDefinition(targetType.Module.Import(interfaceMethodDefinition.ReturnType)));

                    processor.InsertBefore(retCode, processor.Create(OpCodes.Stloc_0));
                    var loadResultIstruction = processor.Create(OpCodes.Ldloc_0);
                    processor.InsertBefore(retCode, loadResultIstruction);
                    processor.InsertBefore(loadResultIstruction, processor.Create(OpCodes.Br_S, loadResultIstruction));
                }
            }

            return md;
        }
    }
}
