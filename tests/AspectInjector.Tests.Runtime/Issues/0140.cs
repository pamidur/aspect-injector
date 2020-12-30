using AspectInjector.Broker;
using System;

namespace AspectInjector.Tests.Runtime.Issues
{
    public class Issue_0140
    {
        [Aspect(Scope.PerInstance)]
        [Injection(typeof(TestAspect))]
        public class TestAspect : Attribute
        {
            [Advice(Kind.Before)]
            public void Before()
            {

            }
        }

        [Aspect(Scope.PerInstance)]
        [Injection(typeof(TestAspect))]
        public class TestAspect2 : Attribute
        {
            [Advice(Kind.Before)]
            public void Before()
            {

            }
        }

        [TestAspect]
        [TestAspect2]
        internal class ArgumentsTests_GenericClassConstructorChainTargetImpl : ArgumentsTests_GenericClassConstructorChainTarget<string>
        {
        }

        [TestAspect]
        internal abstract class ArgumentsTests_GenericClassConstructorChainTarget<T> where T : class
        {
            public ArgumentsTests_GenericClassConstructorChainTarget()
            {
            }

            public ArgumentsTests_GenericClassConstructorChainTarget(int a) : this()
            {
            }

            public ArgumentsTests_GenericClassConstructorChainTarget(int a, int b) : this(a)
            {
            }

            public void Fact()
            {
            }
        }

    }
}
