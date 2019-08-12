using AspectInjector.Broker;
using System;
using Xunit;

namespace AspectInjector.Tests.Runtime.Issues
{
    public class Issue_0098
    {
        [Fact]
        public void Fixed()
        {
            new Target().Variable = new object();
        }

        [Aspect(Scope.PerInstance)]
        [Injection(typeof(TestAspect))]
        private class TestAspect : Attribute
        {
            [Advice(Kind.Before)]
            public void Before()
            {
                Console.WriteLine("Before");
            }

            [Advice(Kind.After)]
            public void After()
            {
                Console.WriteLine("After");
            }
        }

        private class Target
        {

            private object m_variable = null;

            [TestAspect]
            public object Variable
            {
                get
                {

                    try { return m_variable; }
                    catch (Exception e) { throw; }
                }
                set
                {
                    var oldValue = m_variable;
                    var newValue = value;

                    try
                    {
                        m_variable = value;

                        if (oldValue != default && oldValue != newValue)
                        {
                            LocalMethod(this, oldValue);
                        }
                    }
                    catch (Exception)
                    {
                        m_variable = oldValue;

                        throw;
                    }
                }
            }

            private void LocalMethod(object o1, object o2) { }
        }
    }
}
