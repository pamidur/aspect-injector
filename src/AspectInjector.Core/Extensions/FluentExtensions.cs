using AspectInjector.Broker;
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

        public static bool IsFactoryMethod(this MethodDefinition m)
        {
            return m.IsStatic && m.IsPublic
                && m.Name == Constants.AspectFactoryMethodName
                && m.ReturnType.Match(m.Module.TypeSystem.Object)
                && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.Match(StandardType.Type);
        }

        public static Cut LoadAspect(this in Cut cut, AspectDefinition aspect)
        {
            return LoadAspect(cut, aspect, cut.Method, (in Cut c) => c.ThisOrStatic());
        }

        public static Cut LoadAspect(this in Cut cut, AspectDefinition aspect, MethodDefinition method, PointCut accessor)
        {
            FieldReference aspectField;

            var cur_cut = cut;

            if (method.IsStatic || aspect.Scope == Scope.Global)
                aspectField = GetGlobalAspectField(aspect);
            else
            {
                aspectField = GetInstanceAspectField(aspect, method.DeclaringType, cur_cut);
                cur_cut = cur_cut.Here(accessor);
            }

            return cur_cut.Load(aspectField);
        }

        public static Cut CreateAspectInstance(this in Cut cut, AspectDefinition aspect)
        {
            var call = cut.Call(aspect.GetFactoryMethod(), (in Cut arg) => aspect.Factory != null ? arg.TypeOf(aspect.Host) : arg);

            if (aspect.Factory != null)
                call = call.Cast(call.TypeSystem.Object, aspect.Host);

            return call;
        }

        public static void EnsureAspectInitialized(this MethodDefinition target, AspectDefinition aspect)
        {
            if (target.IsStatic || aspect.Scope == Scope.Global)
                _ = GetGlobalAspectField(aspect);
            else
                _ = GetInstanceAspectField(aspect, target.DeclaringType, new Cut(target.Body, target.Body.Instructions[0]));
        }

        private static FieldReference GetInstanceAspectField(AspectDefinition aspect, TypeDefinition source, in Cut cut)
        {
            var type = source;

            var fieldName = $"{Constants.AspectInstanceFieldPrefix}{aspect.Host.FullName}";

            var field = FindField(type.MakeSelfReference(), fieldName);
            if (field == null)
            {
                var fieldDef = new FieldDefinition(fieldName, FieldAttributes.Family, cut.Import(aspect.Host));
                type.Fields.Add(fieldDef);

                field = fieldDef.MakeReference(type.MakeSelfReference());

                InjectInitialization(GetInstanсeAspectsInitializer(type), field, (in Cut c) => c.CreateAspectInstance(aspect));
            }

            return field;
        }

        private static MethodDefinition GetInstanсeAspectsInitializer(TypeDefinition type)
        {
            var instanceAspectsInitializer = type.Methods.FirstOrDefault(m => m.Name == Constants.InstanceAspectsMethodName);

            if (instanceAspectsInitializer == null)
            {
                instanceAspectsInitializer = new MethodDefinition(Constants.InstanceAspectsMethodName,
                    MethodAttributes.Private | MethodAttributes.HideBySig, type.Module.TypeSystem.Void);

                type.Methods.Add(instanceAspectsInitializer);

                instanceAspectsInitializer.Body.Instead((in Cut i) => i.Return());
                instanceAspectsInitializer.Mark(type.Module.ImportStandardType(WellKnownTypes.DebuggerHiddenAttribute));

                var ctors = type.Methods.Where(c => c.IsConstructor && !c.IsStatic).ToList();

                foreach (var ctor in ctors)
                    ctor.Body.AfterEntry((in Cut i) => i.This().Call(instanceAspectsInitializer.MakeReference(type.MakeSelfReference())));
            }

            return instanceAspectsInitializer;
        }


        private static FieldReference GetGlobalAspectField(AspectDefinition aspect)
        {
            var singleton = aspect.Host.Fields.FirstOrDefault(f => f.Name == Constants.AspectGlobalField);

            if (singleton == null)
                throw new InvalidOperationException("Aspect doesn't have global singleton injected.");

            return singleton.MakeReference(aspect.Host);
        }


        private static void InjectInitialization(MethodDefinition initMethod,
            FieldReference field,
            PointCut factory
            )
        {
            initMethod.Body.AfterEntry(
                (in Cut e) => e
                .IfEqual(
                    (in Cut l) => l.This().Load(field),
                    (in Cut r) => r.Null(),// (this.)aspect == null
                    (in Cut pos) => pos.This().Store(field, factory)// (this.)aspect = new aspect()
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
                if (basetype is GenericInstanceType bgit)
                {   //here we're constructing basetype generic reference 

                    Func<TypeReference, TypeReference> resolveGenericArg = ga => type.Module.ImportReference(ga);

                    if (type is GenericInstanceType git)
                    {
                        var origResolveGenericArg = resolveGenericArg;
                        resolveGenericArg = ga => origResolveGenericArg(ga is GenericParameter gp ? git.GenericArguments[gp.Position] : ga);
                    }

                    var gparams = bgit.GenericArguments.Select(resolveGenericArg).ToArray();
                    basetype = type.Module.ImportReference(basetype.Resolve()).MakeGenericInstanceType(gparams);
                }

                return FindField(basetype, name);
            }
            else
                return field.MakeReference(type);

        }
    }
}
