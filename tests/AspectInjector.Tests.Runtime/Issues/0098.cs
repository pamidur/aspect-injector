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
        public class TestAspect : Attribute
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

            private object _variable = null;

            [TestAspect]
            public object Variable
            {
                get
                {

                    try { return _variable; }
                    catch (Exception) { throw; }
                }
                set
                {
                    var oldValue = _variable;
                    var newValue = value;

                    try
                    {
                        _variable = value;

                        if (oldValue != default && oldValue != newValue)
                        {
                            LocalMethod(this, oldValue);
                        }
                    }
                    catch (Exception)
                    {
                        _variable = oldValue;

                        throw;
                    }
                }
            }

            private void LocalMethod(object o1, object o2) { }
        }
    }
}
