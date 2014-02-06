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

        protected MethodReference GetOrCreateMethodProxy(TypeDefinition targetType, TypeDefinition sourceType, MethodDefinition interfaceMethodDefinition)
        {
            var implementation = sourceType.GetInterfaceImplementation(interfaceMethodDefinition);

            var md = new MethodDefinition(interfaceMethodDefinition.DeclaringType.FullName + "." + interfaceMethodDefinition.Name
                , MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual
                , implementation.ReturnType);

            targetType.Methods.Add(md);

            md.Overrides.Add(interfaceMethodDefinition);

            return md;
        }

        protected EventReference GetOrCreateMethodEvent(TypeDefinition targetType, EventReference originalMethod)
        {
            throw new NotImplementedException();
        }

        protected PropertyReference GetOrCreateMethodProperty(TypeDefinition targetType, PropertyReference originalMethod)
        {
            throw new NotImplementedException();
        }

        protected PropertyReference GetOrCreateMethodIndexer(TypeDefinition targetType, PropertyReference originalMethod)
        {
            throw new NotImplementedException();
        }
    }
}