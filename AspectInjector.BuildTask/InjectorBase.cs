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

            var fd = new FieldDefinition(aspectPropertyName, FieldAttributes.Private | FieldAttributes.InitOnly, aspectType);

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

        protected EventReference GetOrCreateMethodEvent(TypeDefinition targetType, EventReference originalMethod)
        {
            throw new NotImplementedException();
        }

        protected PropertyReference GetOrCreateMethodIndexer(TypeDefinition targetType, PropertyReference originalMethod)
        {
            throw new NotImplementedException();
        }

        protected PropertyReference GetOrCreateMethodProperty(TypeDefinition targetType, PropertyReference originalMethod)
        {
            throw new NotImplementedException();
        }

        protected MethodReference GetOrCreateMethodProxy(TypeDefinition targetType, TypeDefinition sourceType, MethodDefinition interfaceMethodDefinition)
        {
            var md = new MethodDefinition(interfaceMethodDefinition.DeclaringType.FullName + "." + interfaceMethodDefinition.Name
                , MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual
                , interfaceMethodDefinition.ReturnType);

            foreach (var parameter in interfaceMethodDefinition.Parameters)
                md.Parameters.Add(parameter);

            foreach (var genericParameter in interfaceMethodDefinition.GenericParameters)
                md.GenericParameters.Add(genericParameter);

            var aspectField = GetOrCreateAspectReference(targetType, sourceType);

            var processor = md.Body.GetILProcessor();
            processor.Append(processor.Create(OpCodes.Nop));
            processor.Append(processor.Create(OpCodes.Ldarg_0));
            processor.Append(processor.Create(OpCodes.Ldfld, aspectField));

            if (interfaceMethodDefinition.Parameters.Count > 0)
                processor.Append(processor.Create(OpCodes.Ldarg_1));

            if (interfaceMethodDefinition.Parameters.Count > 1)
                processor.Append(processor.Create(OpCodes.Ldarg_2));

            if (interfaceMethodDefinition.Parameters.Count > 2)
                processor.Append(processor.Create(OpCodes.Ldarg_3));

            if (interfaceMethodDefinition.Parameters.Count > 3)
            {
                for (int i = 4; i < interfaceMethodDefinition.Parameters.Count + 1; i++)
                {
                    processor.Append(processor.Create(OpCodes.Ldarg_S, (byte)i));
                }
            }

            processor.Append(processor.Create(OpCodes.Callvirt, interfaceMethodDefinition));

            if (!interfaceMethodDefinition.ReturnType.IsType(typeof(void)))
            {
                md.Body.Variables.Add(new VariableDefinition(targetType.Module.Import(typeof(object))));
                processor.Append(processor.Create(OpCodes.Stloc_0));
                processor.Append(processor.Create(OpCodes.Ldloc_0));
            }

            processor.Append(processor.Create(OpCodes.Ret));

            md.Overrides.Add(interfaceMethodDefinition);

            targetType.Methods.Add(md);

            return md;
        }
    }
}