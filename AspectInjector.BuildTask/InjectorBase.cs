using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace AspectInjector.BuildTask
{
    public abstract class InjectorBase
    {
        protected FieldReference GetOrCreateAspectReference(TypeDefinition targetType, TypeDefinition aspectType)
        {
            if (!targetType.IsClass)
                throw new NotSupportedException("Field creation supports only classes.");

            var aspectPropertyName = "__a_" + aspectType.Name;

            var existingField = targetType.Fields.FirstOrDefault(f => f.Name == aspectPropertyName && f.FieldType == aspectType);

            if (existingField != null)
                return existingField;

            var fd = new FieldDefinition(aspectPropertyName, FieldAttributes.Private | FieldAttributes.InitOnly, targetType.Module.Import(aspectType));

            var constructor = targetType.Methods.First(m => m.IsConstructor && !m.Parameters.Any());
            var aspectConstructor = targetType.Module.ImportConstructor(aspectType);

            ILProcessor processor = constructor.Body.GetILProcessor();
            Instruction firstInstruction = constructor.Body.Instructions.First();

            processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldarg_0));
            processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Newobj, aspectConstructor));
            processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Stfld, fd));

            targetType.Fields.Add(fd);

            return fd;
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

            var interfaceMethodRef = targetType.Module.Import(interfaceMethodDefinition);

            foreach (var parameter in interfaceMethodRef.Parameters)
                md.Parameters.Add(parameter);

            foreach (var genericParameter in interfaceMethodRef.GenericParameters)
                md.GenericParameters.Add(genericParameter);

            var aspectField = GetOrCreateAspectReference(targetType, sourceType);

            var processor = md.Body.GetILProcessor();
            processor.Append(processor.Create(OpCodes.Nop));
            processor.Append(processor.Create(OpCodes.Ldarg_0));
            processor.Append(processor.Create(OpCodes.Ldfld, aspectField));

            if (interfaceMethodRef.Parameters.Count > 0)
                processor.Append(processor.Create(OpCodes.Ldarg_1));

            if (interfaceMethodRef.Parameters.Count > 1)
                processor.Append(processor.Create(OpCodes.Ldarg_2));

            if (interfaceMethodRef.Parameters.Count > 2)
                processor.Append(processor.Create(OpCodes.Ldarg_3));

            if (interfaceMethodRef.Parameters.Count > 3)
            {
                for (int i = 4; i < interfaceMethodRef.Parameters.Count + 1; i++)
                {
                    processor.Append(processor.Create(OpCodes.Ldarg_S, (byte)i));
                }
            }

            processor.Append(processor.Create(OpCodes.Callvirt, interfaceMethodRef));

            if (!interfaceMethodRef.ReturnType.IsType(typeof(void)))
            {
                md.Body.InitLocals = true;
                md.Body.Variables.Add(new VariableDefinition(targetType.Module.Import(interfaceMethodRef.ReturnType)));

                processor.Append(processor.Create(OpCodes.Stloc_0));
                var loadresultIstruction = processor.Create(OpCodes.Ldloc_0);
                processor.Append(loadresultIstruction);
                processor.InsertBefore(loadresultIstruction, processor.Create(OpCodes.Br_S, loadresultIstruction));
            }

            processor.Append(processor.Create(OpCodes.Ret));

            md.Overrides.Add(interfaceMethodRef);

            targetType.Methods.Add(md);

            return md;
        }
    }
}