// Copyright © 2014 AspectInjector Team
// Author: Alexander Guly
// Licensed under the Apache License, Version 2.0

using AspectInjector.Broker;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace AspectInjector.BuildTask.Contexts
{
    public class TargetTypeContext
    {
        private static readonly string InstanceAspectsInitializerMethodName = "__a$_initializeInstanceAspects";
        private static readonly string TypeAspectsInitializerMethodName = "__a$_initializeTypeAspects";
        private static readonly string AspectPropertyNamePrefix = "__a$_";

        public TypeDefinition TypeDefinition { get; private set; }

        private TargetMethodContext[] _constructors;

        private TargetMethodContext _typeAspectsInitializer = null;
        private TargetMethodContext _instanceAspectsInitializer = null;

        public TargetTypeContext(TypeDefinition typeDefinition, TargetMethodContext[] ctorCtx)
        {
            TypeDefinition = typeDefinition;
            _constructors = ctorCtx;
        }

        private TargetMethodContext TypeAspectsInitializer
        {
            get
            {
                if (_typeAspectsInitializer == null)
                {
                    _typeAspectsInitializer = CreateMethod(TypeAspectsInitializerMethodName,
                        MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig,
                        TypeDefinition.Module.TypeSystem.Void);

                    var cctor = _constructors.FirstOrDefault(c => c.TargetMethod.IsStatic) ?? CreateStaticConstructor();
                    cctor.EntryPoint.InjectMethodCall(_typeAspectsInitializer.TargetMethod, new object[] { });
                }

                return _typeAspectsInitializer;
            }
        }

        private TargetMethodContext InstanсeAspectsInitializer
        {
            get
            {
                if (_instanceAspectsInitializer == null)
                {
                    _instanceAspectsInitializer = CreateMethod(InstanceAspectsInitializerMethodName,
                        MethodAttributes.Private | MethodAttributes.HideBySig,
                        TypeDefinition.Module.TypeSystem.Void);

                    var ctors = _constructors.Where(c => !c.TargetMethod.IsStatic).ToList();

                    foreach (var ctor in ctors)
                    {
                        ctor.EntryPoint.LoadSelfOntoStack();
                        ctor.EntryPoint.InjectMethodCall(_instanceAspectsInitializer.TargetMethod, new object[] { });
                    }
                }

                return _instanceAspectsInitializer;
            }
        }

        public TargetMethodContext CreateMethod(string name, MethodAttributes attrs, TypeReference returnType)
        {
            var method = new MethodDefinition(name, attrs, TypeDefinition.Module.Import(returnType));
            var processor = method.Body.GetILProcessor();
            processor.Append(processor.Create(OpCodes.Nop));
            processor.Append(processor.Create(OpCodes.Ret));
            TypeDefinition.Methods.Add(method);
            return MethodContextFactory.GetOrCreateContext(method);
        }

        public FieldReference GetOrCreateAspectReference(AspectContext info)
        {
            if (info.TargetTypeContext != this)
                throw new NotSupportedException("Aspect info mismatch.");

            var aspectPropertyName = AspectPropertyNamePrefix + info.AdviceClassType.Name;

            var existingField = TypeDefinition.Fields.FirstOrDefault(f => f.Name == aspectPropertyName && f.FieldType.IsTypeOf(info.AdviceClassType));
            if (existingField != null)
                return existingField;

            TargetMethodContext initMethod = null;
            FieldAttributes fieldAttrs;

            if (info.AdviceClassScope == AspectScope.Instance)
            {
                fieldAttrs = FieldAttributes.Private;
                initMethod = InstanсeAspectsInitializer;
            }
            else if (info.AdviceClassScope == AspectScope.Type)
            {
                fieldAttrs = FieldAttributes.Private | FieldAttributes.Static;
                initMethod = TypeAspectsInitializer;
            }
            else throw new NotSupportedException("Scope " + info.AdviceClassScope.ToString() + " is not supported (yet).");

            var field = new FieldDefinition(aspectPropertyName, fieldAttrs, TypeDefinition.Module.Import(info.AdviceClassType));
            TypeDefinition.Fields.Add(field);

            InjectInitialization(initMethod, field, info.AdviceClassFactory);

            return field;
        }

        private TargetMethodContext CreateStaticConstructor()
        {
            return CreateMethod(".cctor",
                MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                TypeDefinition.Module.TypeSystem.Void);
        }

        private void InjectInitialization(TargetMethodContext initMethod,
            FieldDefinition field,
            MethodDefinition factoryMethod)
        {
            var point = initMethod.EntryPoint;
            var proc = initMethod.EntryPoint.Processor;

            var endBlock = proc.Create(OpCodes.Nop);

            point.LoadFieldOntoStack(field);
            point.InsertBefore(proc.Create(OpCodes.Ldnull));
            point.InsertBefore(proc.Create(OpCodes.Ceq));
            point.InsertBefore(proc.Create(OpCodes.Ldc_I4_0));
            point.InsertBefore(proc.Create(OpCodes.Ceq));
            point.InsertBefore(proc.Create(OpCodes.Brtrue_S, endBlock));

            point.InjectMethodCall(factoryMethod, new object[] { });
            point.SetFieldFromStack(field);

            point.InsertBefore(endBlock);
        }
    }
}