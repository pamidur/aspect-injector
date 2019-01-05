using AspectInjector.Broker;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent;
using AspectInjector.Rules;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Models
{
    public class AspectDefinition
    {
        private MethodDefinition _factoryMethod;

        public TypeDefinition Host { get; set; }

        public List<Effect> Effects { get; set; }

        public Scope Scope { get; set; }

        public TypeReference Factory { get; set; }

        private MethodReference GetFactoryMethod()
        {
            if (_factoryMethod == null)
            {
                if (Factory != null)
                {
                    _factoryMethod = Factory.Resolve().Methods.FirstOrDefault(m =>
                    m.IsStatic && m.IsPublic
                    && m.Name == Constants.AspectFactoryMethodName
                    && m.ReturnType.FullName == WellKnownTypes.Object
                    && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName == WellKnownTypes.Type
                    );
                }
                else
                    _factoryMethod = Host.Methods.FirstOrDefault(m => m.IsConstructor && !m.IsStatic && m.IsPublic && !m.HasParameters);
            }

            return _factoryMethod;
        }

        public void CreateAspectInstance(PointCut c)
        {
            c = c.Call(GetFactoryMethod(), arg =>
            {
                if (Factory != null)
                    arg.TypeOf(Host);
            });

            if (Factory != null)
                c.Cast(Host.Module.GetTypeSystem().Object, Host);
        }

        public bool Validate(ILogger log)
        {
            var result = true;

            if(Scope!= Scope.Global && Scope!= Scope.PerInstance)
                log.Log(GeneralRules.UnknownCompilationOption, Host, GeneralRules.Literals.UnknownAspectScope(Scope.ToString()));

            if (!Effects.Any())
                log.Log(AspectRules.AspectShouldContainEffect, Host, Host.Name);

            if (Host.HasGenericParameters)
            {
                log.Log(AspectRules.AspectMustHaveValidSignature, Host, Host.Name, AspectRules.Literals.HasGenericParams);
                result = false;
            }

            if (Host.IsAbstract)
            {
                if (Host.IsSealed)
                    log.Log(AspectRules.AspectMustHaveValidSignature, Host, Host.Name, AspectRules.Literals.IsStatic);
                else
                    log.Log(AspectRules.AspectMustHaveValidSignature, Host, Host.Name, AspectRules.Literals.IsAbstract);

                result = false;
            }

            if (GetFactoryMethod() == null)
            {
                if (Factory != null)
                    log.Log(AspectRules.AspectFactoryMustContainFactoryMethod, Host, Factory.Name);
                else
                    log.Log(AspectRules.AspectMustHaveContructorOrFactory, Host, Host.Name);

                result = false;
            }

            var effectsValid = Effects.All(e => e.Validate(this, log));

            return result && effectsValid;
        }
    }
}