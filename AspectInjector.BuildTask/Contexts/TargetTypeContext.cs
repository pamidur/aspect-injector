// Copyright © 2014 AspectInjector Team
// Author: Alexander Guly
// Licensed under the Apache License, Version 2.0

using AspectInjector.Broker;
using AspectInjector.BuildTask.Common;
using AspectInjector.BuildTask.Processors.ModuleProcessors;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.BuildTask.Contexts
{
    public class TargetTypeContext
    {
        private static readonly string _perTypeAspectInitializerMethodName = "__a$_initializePerTypeAspects";

        public TypeDefinition TypeDefinition { get; private set; }

        private TargetMethodContext[] _constructors;

        private bool _isAspectsInitialized = false;

        private TargetMethodContext _perTypeAspectsInitializer = null;
        private TargetMethodContext _perInstanseAspectsInitializer = null;

        public TargetTypeContext(TypeDefinition typeDefinition, TargetMethodContext[] ctorCtx)
        {
            TypeDefinition = typeDefinition;
            _constructors = ctorCtx;
        }

        public TargetMethodContext CreateMethod(string name, MethodAttributes attrs, TypeReference returnType)
        {
            var method = new MethodDefinition(name, attrs, returnType);
            var processor = method.Body.GetILProcessor();
            processor.Append(processor.Create(OpCodes.Nop));
            processor.Append(processor.Create(OpCodes.Ret));
            TypeDefinition.Methods.Add(method);
            return MethodContextFactory.GetOrCreateContext(method);
        }

        private TargetMethodContext PerTypeAspectsInitializer
        {
            get
            {
                if (_perTypeAspectsInitializer != null)
                    return _perTypeAspectsInitializer;

                _perTypeAspectsInitializer = CreateMethod(_perTypeAspectInitializerMethodName, MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, TypeDefinition.Module.TypeSystem.Void);

                var cctor = _constructors.FirstOrDefault(c => c.TargetMethod.IsStatic);

                if (cctor != null)
                    cctor.InjectMethodCall(cctor.EntryPoint, null, _perTypeAspectsInitializer.TargetMethod, new object[] { });

                return _perTypeAspectsInitializer;
            }
        }

        private TargetMethodContext PerInstanseAspectsInitializer
        {
            get
            {
                if (_perInstanseAspectsInitializer != null)
                    return _perInstanseAspectsInitializer;

                _perInstanseAspectsInitializer = CreateMethod(_perTypeAspectInitializerMethodName, MethodAttributes.Private | MethodAttributes.HideBySig, TypeDefinition.Module.TypeSystem.Void);

                var ctors = _constructors.Where(c => !c.TargetMethod.IsStatic).ToList();

                foreach (var ctor in ctors)
                    ctor.InjectMethodCall(ctor.EntryPoint, null, _perInstanseAspectsInitializer.TargetMethod, new object[] { });

                return _perInstanseAspectsInitializer;
            }
        }

        public FieldReference GetOrCreateAspectReference(AspectInjectionInfo info)
        {
            if (info.TargetTypeContext != this)
                throw new NotSupportedException("Aspect info mismatch.");

            var aspectPropertyName = "__a$_" + info.AspectType.Name;

            var existingField = TypeDefinition.Fields.FirstOrDefault(f => f.Name == aspectPropertyName && f.FieldType == info.AspectType);
            if (existingField != null)
                return existingField;

            TargetMethodContext initMethod = null;
            FieldAttributes fieldAttrs;
            MethodDefinition factory;
            var factoryArgs = new List<object>();

            if (info.AspectScope == AspectScope.PerInstanse)
            {
                fieldAttrs = FieldAttributes.Private;
                initMethod = PerInstanseAspectsInitializer;

                var ctors = info.AspectType.Methods.Where(c => c.IsConstructor && !c.IsStatic && c.Parameters.Count == 0).ToList();

                if (ctors.Count == 0)
                    throw new CompilationException("Cannot find empty constructor for aspect.", info.AspectType);

                factory = ctors.First();
            }
            else if (info.AspectScope == AspectScope.PerType)
            {
                fieldAttrs = FieldAttributes.Private | FieldAttributes.Static;
                initMethod = PerTypeAspectsInitializer;

                factory = TypeDefinition.Module.Types.First(t => t.Namespace == SnippetsProcessor.SnippetsNamespace && t.Name == "AspectFactory")
                    .Methods.First(m => m.Name == "GetPerTypeAspect");
            }
            else throw new NotSupportedException("Scope " + info.AspectScope.ToString() + " is not supported.");

            var fd = new FieldDefinition(aspectPropertyName, fieldAttrs, TypeDefinition.Module.Import(info.AspectType));
            TypeDefinition.Fields.Add(fd);

            var proc = initMethod.Processor;
            var point = initMethod.EntryPoint;

            var blockend = proc.Create(OpCodes.Nop);

            initMethod.LoadFieldOntoStack(point, fd);
            proc.InsertBefore(point, proc.Create(OpCodes.Ldnull));
            proc.InsertBefore(point, proc.Create(OpCodes.Ceq));
            proc.InsertBefore(point, proc.Create(OpCodes.Ldc_I4_0));
            proc.InsertBefore(point, proc.Create(OpCodes.Ceq));
            proc.InsertBefore(point, proc.Create(OpCodes.Brtrue_S, blockend));
            {
                initMethod.SetFieldFromStack(point, fd, () => initMethod.InjectMethodCall(point, null, factory, factoryArgs.ToArray()));
            }
            proc.InsertBefore(point, blockend);

            return fd;
        }
    }
}