using AspectInjector.Broker;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace AspectInjector.BuildTask
{
    internal class AdviceInterfaceProxyInjector : InjectorBase, IModuleProcessor
    {
        public void ProcessModule(ModuleDefinition module)
        {
            var allClasses = module.Types
                .Where(t => t.IsClass && t.CustomAttributes.HasAttributeOfType<AspectAttribute>());

            foreach (var @class in allClasses)
            {
                var aspects = from ca in @class.CustomAttributes
                              where ca.IsAttributeOfType<AspectAttribute>()
                              let type = ((TypeReference)ca.ConstructorArguments[0].Value).Resolve()
                              where type.CustomAttributes.Any(tca => tca.IsAttributeOfType<AdviceInterfaceProxyAttribute>())
                              select new { aspect = ca, type };

                if (!aspects.Any())
                    continue;

                var uniqueAspects = aspects.GroupBy(p => p.type).Select(g => g.First());

                foreach (var aspect in uniqueAspects)
                {
                    //todo:: process other aspect parameters

                    InjectAspectIntoClass(@class, aspect.type);
                }
            }
        }

        protected virtual void InjectAspectIntoClass(TypeDefinition classDefinition, TypeDefinition aspectDefinition)
        {
            var interfaceInjectionDefinitions = from ca in aspectDefinition.CustomAttributes
                                                where ca.IsAttributeOfType<AdviceInterfaceProxyAttribute>()
                                                select new { @interface = (TypeReference)ca.ConstructorArguments[0].Value };

            foreach (var interfaceInjectionDefinition in interfaceInjectionDefinitions)
            {
                var interfaceReference = interfaceInjectionDefinition.@interface;
                //todo:: process other InterfaceProxyInjectionAttribute parameters        

                InjectInterfaceProxyIntoClass(classDefinition, aspectDefinition, interfaceReference.Resolve());
            }
        }

        protected virtual void InjectInterfaceProxyIntoClass(TypeDefinition classDefinition, TypeDefinition aspectDefinition, TypeDefinition interfaceDefinition)
        {
            if (!interfaceDefinition.IsInterface)
                throw new CompilationException(interfaceDefinition.Name + " is not an interface on interface injection definition on acpect " + aspectDefinition.Name, aspectDefinition);

            if (!aspectDefinition.ImplementsInterface(interfaceDefinition))
                throw new CompilationException(aspectDefinition.Name + " should implement " + interfaceDefinition.Name, aspectDefinition);

            if (!classDefinition.ImplementsInterface(interfaceDefinition))
            {
                var ifaces = interfaceDefinition.GetInterfacesTree();

                foreach (var iface in ifaces)
                    classDefinition.Interfaces.Add(classDefinition.Module.Import(iface));
            }
            else if (!classDefinition.Interfaces.Contains(interfaceDefinition))
            {
                //In order to behave the same as csc
                classDefinition.Interfaces.Add(interfaceDefinition);
            }

            var methods = interfaceDefinition.GetInterfaceTreeMemebers(td => td.Methods).Where(m => !m.IsAddOn && !m.IsRemoveOn && !m.IsSetter && !m.IsGetter);
            var properties = interfaceDefinition.GetInterfaceTreeMemebers(td => td.Properties);
            var events = interfaceDefinition.GetInterfaceTreeMemebers(td => td.Events);

            foreach (var method in methods)
                GetOrCreateMethodProxy(classDefinition, aspectDefinition, method);

            foreach (var @event in events)
                GetOrCreateEventProxy(classDefinition, aspectDefinition, @event);

            foreach (var property in properties)
                GetOrCreatePropertyProxy(classDefinition, aspectDefinition, property);
        }

        protected PropertyDefinition GetOrCreatePropertyProxy(TypeDefinition targetType, TypeDefinition sourceType, PropertyDefinition originalProperty)
        {
            var newGetMethod = GetOrCreateMethodProxy(targetType, sourceType, originalProperty.GetMethod);
            var newSetMethod = GetOrCreateMethodProxy(targetType, sourceType, originalProperty.SetMethod);

            var pd = new PropertyDefinition(originalProperty.Name, PropertyAttributes.None, targetType.Module.Import(originalProperty.PropertyType));
            pd.GetMethod = newGetMethod;
            pd.SetMethod = newSetMethod;

            targetType.Properties.Add(pd);

            return pd;
        }

        protected EventDefinition GetOrCreateEventProxy(TypeDefinition targetType, TypeDefinition sourceType, EventDefinition originalEvent)
        {
            var newAddMethod = GetOrCreateMethodProxy(targetType, sourceType, originalEvent.AddMethod);
            var newRemoveMethod = GetOrCreateMethodProxy(targetType, sourceType, originalEvent.RemoveMethod);

            var ed = new EventDefinition(originalEvent.Name, EventAttributes.None, targetType.Module.Import(originalEvent.EventType));
            ed.AddMethod = newAddMethod;
            ed.RemoveMethod = newRemoveMethod;

            targetType.Events.Add(ed);

            return ed;
        }

        protected MethodDefinition GetOrCreateMethodProxy(TypeDefinition targetType, TypeDefinition sourceType, MethodDefinition interfaceMethodDefinition)
        {
            var methodName = interfaceMethodDefinition.DeclaringType.FullName + "." + interfaceMethodDefinition.Name;

            var existedMethod = targetType.Methods.FirstOrDefault(m => m.Name == methodName && m.SignatureMatches(interfaceMethodDefinition));
            if (existedMethod != null)
                return existedMethod;

            var md = new MethodDefinition(methodName
                , MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual
                , targetType.Module.Import(interfaceMethodDefinition.ReturnType));

            if (interfaceMethodDefinition.IsSpecialName)
                md.IsSpecialName = true;

            md.Overrides.Add(targetType.Module.Import(interfaceMethodDefinition));
            targetType.Methods.Add(md);

            foreach (var parameter in interfaceMethodDefinition.Parameters)
                md.Parameters.Add(parameter);

            foreach (var genericParameter in interfaceMethodDefinition.GenericParameters)
                md.GenericParameters.Add(genericParameter);

            var aspectField = GetOrCreateAspectReference(targetType, sourceType);

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
                var loadresultIstruction = processor.Create(OpCodes.Ldloc_0);
                processor.InsertBefore(retCode, loadresultIstruction);
                processor.InsertBefore(loadresultIstruction, processor.Create(OpCodes.Br_S, loadresultIstruction));
            }

            return md;
        }
    }
}