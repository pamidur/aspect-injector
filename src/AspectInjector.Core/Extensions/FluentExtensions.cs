using AspectInjector.Broker;
using AspectInjector.Core.Models;
using FluentIL;
using FluentIL.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace AspectInjector.Core.Extensions
{
    public static class FluentExtensions
    {
        public static void OnAspectsInitialized(this MethodBody body, PointCut action)
        {
            var method = body.Method;
            if (!method.HasBody) return;

            if (!method.IsConstructor || method.IsStatic)
            {
                body.AfterEntry(action);
                return;
            }

            MethodReference initializer = method.DeclaringType.Methods.FirstOrDefault(m => m.Name == Constants.InstanceAspectsMethodName);

            if (initializer == null)
            {
                body.AfterEntry(action);
                return;
            }

            initializer = initializer.MakeHostInstanceGeneric(method.DeclaringType);
            body.OnCall(initializer, action);
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
                cut = cut.Cast(cut.TypeSystem.Object, aspect.Host);

            return cut;
        }

        private static FieldReference GetInstanceAspectField(AspectDefinition aspect, TypeDefinition source, Cut cut)
        {
            var type = source;

            var fieldName = $"{Constants.AspectInstanceFieldPrefix}{aspect.Host.FullName}";

            var field = FindField(type, fieldName);
            if (field == null)
            {
                field = new FieldDefinition(fieldName, FieldAttributes.Family, cut.TypeSystem.Import(aspect.Host));
                type.Fields.Add(field);

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
                    MethodAttributes.Private | MethodAttributes.HideBySig, cut.TypeSystem.Void);

                type.Methods.Add(instanceAspectsInitializer);

                instanceAspectsInitializer.Body.Instead(i => i.Return());
                instanceAspectsInitializer.Mark(cut.TypeSystem.DebuggerHiddenAttribute);

                var ctors = type.Methods.Where(c => c.IsConstructor && !c.IsStatic).ToList();

                foreach (var ctor in ctors)
                    ctor.Body.AfterEntry(i => i.This().Call(instanceAspectsInitializer.MakeHostInstanceGeneric(cut.Method.DeclaringType)));
            }

            return instanceAspectsInitializer;
        }


        private static FieldReference GetGlobalAspectField(AspectDefinition aspect)
        {
            var singleton = aspect.Host.Fields.FirstOrDefault(f => f.Name == Constants.AspectGlobalField);

            if (singleton == null)
                throw new Exception("Missed aspect global singleton.");

            return singleton;
        }


        private static void InjectInitialization(MethodDefinition initMethod,
            FieldDefinition field,
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

        private static FieldDefinition FindField(TypeDefinition type, string name)
        {
            if (type == null)
                return null;

            var field = type.Fields.FirstOrDefault(f => f.Name == name);
            return field ?? FindField(type.BaseType?.Resolve(), name);
        }
    }
}
