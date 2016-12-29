using AspectInjector.Broker;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Core.Fluent
{
    internal class AspectFactory
    {
        private readonly string _instanceAspectsInitializerMethodName;
        private readonly string _typeAspectsInitializerMethodName;

        private readonly EditorContext _ctx;

        public AspectFactory(EditorContext ctx)
        {
            _ctx = ctx;

            _typeAspectsInitializerMethodName = $"{_ctx.Factory.Prefix}initializeTypeAspects";
            _instanceAspectsInitializerMethodName = $"{_ctx.Factory.Prefix}initializeInstanceAspects";
        }

        private MethodDefinition GetTypeAspectsInitializer(TypeDefinition from)
        {
            var typeAspectsInitializer = from.Methods.FirstOrDefault(m => m.Name == _typeAspectsInitializerMethodName);

            if (typeAspectsInitializer == null)
            {
                typeAspectsInitializer = CreateMethod(from, _typeAspectsInitializerMethodName,
                    MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, _ctx.TypeSystem.Void);

                var cctor = from.Methods.FirstOrDefault(c => c.IsConstructor && c.IsStatic) ?? CreateStaticConstructor(from);

                _ctx.Factory.GetEditor(cctor).OnInit(i => i.Call(typeAspectsInitializer));
            }

            return typeAspectsInitializer;
        }

        private MethodDefinition GetInstanсeAspectsInitializer(TypeDefinition from)
        {
            var instanceAspectsInitializer = from.Methods.FirstOrDefault(m => m.Name == _instanceAspectsInitializerMethodName);

            if (instanceAspectsInitializer == null)
            {
                instanceAspectsInitializer = CreateMethod(from, _instanceAspectsInitializerMethodName,
                    MethodAttributes.Private | MethodAttributes.HideBySig, _ctx.TypeSystem.Void);

                var ctors = from.Methods.Where(c => c.IsConstructor && !c.IsStatic).ToList();

                foreach (var ctor in ctors)
                    _ctx.Factory.GetEditor(ctor).OnInit(i => i.This().Call(instanceAspectsInitializer));
            }

            return instanceAspectsInitializer;
        }

        private MethodDefinition CreateMethod(TypeDefinition td, string name, MethodAttributes attrs, TypeReference returnType)
        {
            var method = new MethodDefinition(name, attrs, _ctx.TypeSystem.Import(returnType));
            var processor = _ctx.Factory.GetProcessor(method.Body);
            processor.Append(processor.Create(OpCodes.Ret));
            td.Methods.Add(method);
            return method;
        }

        public FieldDefinition Get(AspectUsage aspect, TypeDefinition target)
        {
            MethodDefinition initMethod = null;
            FieldAttributes fieldAttrs;
            string aspectPropertyName = null;

            if (aspect.Scope == AspectCreationScope.Instance)
            {
                fieldAttrs = FieldAttributes.Private;
                initMethod = GetInstanсeAspectsInitializer(target);
                aspectPropertyName = $"{_ctx.Factory.Prefix}i_{aspect.InjectionHost.Name}";
            }
            else if (aspect.Scope == AspectCreationScope.Type)
            {
                fieldAttrs = FieldAttributes.Private | FieldAttributes.Static;
                initMethod = GetTypeAspectsInitializer(target);
                aspectPropertyName = $"{_ctx.Factory.Prefix}t_{aspect.InjectionHost.Name}";
            }
            else throw new NotSupportedException("Scope " + aspect.Scope.ToString() + " is not supported (yet).");

            var existingField = target.Fields.FirstOrDefault(f => f.Name == aspectPropertyName && f.FieldType.IsTypeOf(aspect.InjectionHost));
            if (existingField != null)
                return existingField;

            var field = new FieldDefinition(aspectPropertyName, fieldAttrs, _ctx.TypeSystem.Import(aspect.InjectionHost));
            target.Fields.Add(field);

            InjectInitialization(initMethod, field, aspect.InjectionHostFactory);

            return field;
        }

        private MethodDefinition CreateStaticConstructor(TypeDefinition td)
        {
            return CreateMethod(td, ".cctor",
                MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                _ctx.TypeSystem.Void);
        }

        private void InjectInitialization(MethodDefinition initMethod,
            FieldDefinition field,
            MethodDefinition factoryMethod)
        {
            _ctx.Factory.GetEditor(initMethod).OnEntry(e =>
            {
                e.If(c => // (this.)aspect == null
                {
                    if (!field.IsStatic)
                        c.This();
                    c.Load(field);

                    c.Value((object)null);
                },
                pos => // (this.)aspect = new aspect()
                {
                    if (!field.IsStatic)
                        pos.This();

                    pos.Store(field, val => val.Call(factoryMethod));
                });
            });
        }
    }
}