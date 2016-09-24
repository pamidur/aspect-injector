using System;
using AspectInjector.Broker;
using AspectInjector.CompileTimeTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;

namespace AspectInjector.CompileTimeTests.InjectionProcessor
{
    public abstract class AInjectionProcessorTest
    {
        protected ModuleDefinition Module;

        [TestInitialize]
        public void Init()
        {
            var generator = new TestAssemblyGenerator(GetType());
            Module = generator.CreateTestAssembly().MainModule;
        }

        protected class TestAspectImplementation
        {
            [Advice(InjectionPoints.Around, InjectionTargets.Constructor)]
            public object AroundCtor([AdviceArgument(AdviceArgumentSource.Target)] Func<object[], object> target,
                [AdviceArgument(AdviceArgumentSource.Arguments)] object[] arguments)
            {
                return new object();
            }

            [Advice(InjectionPoints.Around, InjectionTargets.Method)]
            public object AroundMethod([AdviceArgument(AdviceArgumentSource.Target)] Func<object[], object> target,
                [AdviceArgument(AdviceArgumentSource.Arguments)] object[] arguments)
            {
                return new object();
            }

            [Advice(InjectionPoints.Around, InjectionTargets.Getter)]
            public object AroundProperty([AdviceArgument(AdviceArgumentSource.Target)] Func<object[], object> target)
            {
                return new object();
            }

            [Advice(InjectionPoints.Around, InjectionTargets.EventAdd)]
            public object AroundEvent([AdviceArgument(AdviceArgumentSource.Target)] Func<object[], object> target)
            {
                return new object();
            }
        }
    }
}