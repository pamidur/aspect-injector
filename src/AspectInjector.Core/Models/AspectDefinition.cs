using AspectInjector.Broker;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;
using static AspectInjector.Broker.Aspect;

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
                c.Cast(Host);
        }

        public bool Validate(ILogger log)
        {
            if (!Effects.Any())
                log.LogWarning(CompilationMessage.From($"Type {Host.FullName} has defined as an aspect, but lacks any effect.", Host));

            if (Host.HasGenericParameters)
            {
                log.LogError(CompilationMessage.From($"Aspect {Host.FullName} should not have generic parameters.", Host));
                return false;
            }

            if (Host.IsAbstract)
            {
                log.LogError(CompilationMessage.From($"Aspect {Host.FullName} cannot be static nor abstract.", Host));
                return false;
            }

            if (GetFactoryMethod() == null)
            {
                if (Factory != null)
                    log.LogError(CompilationMessage.From($"Type {Factory.FullName} should have 'public static object GetInstance(Type)' method in order to be aspect factory.", Host));
                else
                    log.LogError(CompilationMessage.From($"Aspect {Host.FullName} has no parameterless public constructor nor valid factory.", Host.Methods.First(m => m.IsConstructor && !m.IsStatic)));
                return false;
            }

            return Effects.All(e => e.Validate(this, log));
        }
    }
}