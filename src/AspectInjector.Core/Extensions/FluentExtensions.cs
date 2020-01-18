﻿using AspectInjector.Broker;
using AspectInjector.Core.Models;
using FluentIL;
using FluentIL.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Linq;

namespace AspectInjector.Core.Extensions
{
    public static class FluentExtensions
    {
        public static Instruction GetUserCodeStart(this MethodBody body)
        {
            var method = body.Method;
            if (!method.HasBody) return null;

            if (!method.IsConstructor || method.IsStatic)
                return body.GetCodeStart();

            var initializer = method.DeclaringType.Methods.FirstOrDefault(m => m.Name == Constants.InstanceAspectsMethodName);

            if (initializer == null)
                return body.GetCodeStart();

            var initializerRef = initializer.MakeReference(method.DeclaringType.MakeSelfReference());

            var instruction = body.Instructions.FirstOrDefault(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference mref && mref.DeclaringType.Match(initializerRef.DeclaringType) && mref.Resolve() == initializer);
            return instruction.Next;
        }

        public static void OnAspectsInitialized(this MethodBody body, PointCut action)
        {
            var userCodesStart = body.GetUserCodeStart();
            if (userCodesStart == null) return;

            body.BeforeInstruction(userCodesStart, action);
        }

        public static Cut LoadAspect(this Cut cut, AspectDefinition aspect)
        {
            return LoadAspect(cut, aspect, cut.Method, c => c.ThisOrStatic());
        }

        public static Cut LoadAspect(this Cut cut, AspectDefinition aspect, MethodDefinition method, PointCut accessor)
        {
            FieldReference aspectField;

            if (method.IsStatic || aspect.Scope == Scope.Global)
                aspectField = GetGlobalAspectField(aspect);
            else
            {
                aspectField = GetInstanceAspectField(aspect, method.DeclaringType, cut);
                cut = cut.Here(accessor);
            }

            return cut.Load(aspectField);
        }

        public static Cut CreateAspectInstance(this Cut cut, AspectDefinition aspect)
        {
            cut = cut.Call(aspect.GetFactoryMethod(), arg => aspect.Factory != null ? arg.TypeOf(aspect.Host) : arg);

            if (aspect.Factory != null)
                cut = cut.Cast(StandardTypes.Object, aspect.Host);

            return cut;
        }

        private static FieldReference GetInstanceAspectField(AspectDefinition aspect, TypeDefinition source, Cut cut)
        {
            var type = source;

            var fieldName = $"{Constants.AspectInstanceFieldPrefix}{aspect.Host.FullName}";

            var field = FindField(type.MakeSelfReference(), fieldName);
            if (field == null)
            {
                var fieldDef = new FieldDefinition(fieldName, FieldAttributes.Family, cut.Import(aspect.Host));
                type.Fields.Add(fieldDef);

                field = fieldDef.MakeReference(type.MakeSelfReference());

                InjectInitialization(GetInstanсeAspectsInitializer(type, cut), field, c => c.CreateAspectInstance(aspect));
            }

            return field;
        }

        private static MethodDefinition GetInstanсeAspectsInitializer(TypeDefinition type, Cut cut)
        {
            var instanceAspectsInitializer = type.Methods.FirstOrDefault(m => m.Name == Constants.InstanceAspectsMethodName);

            if (instanceAspectsInitializer == null)
            {
                instanceAspectsInitializer = new MethodDefinition(Constants.InstanceAspectsMethodName,
                    MethodAttributes.Private | MethodAttributes.HideBySig, type.Module.ImportReference(StandardTypes.Void));

                type.Methods.Add(instanceAspectsInitializer);

                instanceAspectsInitializer.Body.Instead(i => i.Return());
                instanceAspectsInitializer.Mark(WellKnownTypes.DebuggerHiddenAttribute);

                var ctors = type.Methods.Where(c => c.IsConstructor && !c.IsStatic).ToList();

                foreach (var ctor in ctors)
                    ctor.Body.AfterEntry(i => i.This().Call(instanceAspectsInitializer.MakeReference(type.MakeSelfReference())));
            }

            return instanceAspectsInitializer;
        }


        private static FieldReference GetGlobalAspectField(AspectDefinition aspect)
        {
            var singleton = aspect.Host.Fields.FirstOrDefault(f => f.Name == Constants.AspectGlobalField);

            if (singleton == null)
                throw new Exception("Missed aspect global singleton.");

            return singleton.MakeReference(aspect.Host);
        }


        private static void InjectInitialization(MethodDefinition initMethod,
            FieldReference field,
            PointCut factory
            )
        {
            initMethod.Body.AfterEntry(
                e => e
                .IfEqual(
                    l => l.This().Load(field),
                    r => r.Null(),// (this.)aspect == null
                    pos => pos.This().Store(field, factory)// (this.)aspect = new aspect()
                )
            );
        }

        private static FieldReference FindField(TypeReference type, string name)
        {
            if (type == null)
                return null;

            var field = type.Resolve().Fields.FirstOrDefault(f => f.Name == name && (f.Attributes & FieldAttributes.Family) != 0);

            if (field == null)
            {
                var basetype = type.Resolve().BaseType;
                if (basetype is GenericInstanceType git)
                {
                    var gtype = type as GenericInstanceType;
                    var gparams = git.GenericArguments.Select(ga =>
                    {
                        if (ga is GenericParameter gp && gtype != null)
                            return type.Module.ImportReference(gtype.GenericArguments[gp.Position]);
                        else
                            return type.Module.ImportReference(ga);

                    }).ToArray();

                    basetype = type.Module.ImportReference(basetype.Resolve()).MakeGenericInstanceType(gparams);
                }

                return FindField(basetype, name);
            }
            else
                return field.MakeReference(type);

        }
    }
}
