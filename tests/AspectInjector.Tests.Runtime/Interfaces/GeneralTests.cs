using AspectInjector.Broker;
using System;
using System.ComponentModel;
using Xunit;

namespace AspectInjector.Tests.Interfaces
{

    public class GeneralTests
    {
        private IGeneralTests _testClass;
        private GeneralTests_Target2 _testClass2;

        public GeneralTests()
        {
            _testClass = (IGeneralTests)new GeneralTests_Target();
            _testClass2 = new GeneralTests_Target2();
        }

        [Fact]
        public void Inject_Mixin_If_Injected_into_Member()
        {
            Assert.True(_testClass2 is IGeneralTests);
        }

        [Fact]
        public void Interfaces_InjectAspectReference_SecondConstructor()
        {
            object ref1 = new object();
            object out1;
            int ref2 = 2;
            int out2;

            Checker.Passed = false;
            var result = ((IGeneralTests)new GeneralTests_Target(1)).Fact("ok", 1, ref ref1, out out1, ref ref2, out out2);
            Assert.Equal("ok", result);
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Interfaces_InjectMethodProxy()
        {
            object ref1 = new object();
            object out1;
            int ref2 = 2;
            int out2;

            Checker.Passed = false;
            var result = _testClass.Fact("ok", 1, ref ref1, out out1, ref ref2, out out2);
            Assert.Equal("ok", result);
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Interfaces_InjectSetterProxy()
        {
            Checker.Passed = false;
            _testClass.TestProperty = 4;
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Interfaces_InjectGetterProxy()
        {
            Checker.Passed = false;
            var a = _testClass.TestProperty;
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Interfaces_InjectEventAddProxy()
        {
            Checker.Passed = false;
            _testClass.TestEvent += (s, e) => { };
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Interfaces_InjectEventRemoveProxy()
        {
            Checker.Passed = false;
            _testClass.TestEvent -= (s, e) => { };
            Assert.True(Checker.Passed);
        }

        [GeneralTests_Trigger]
        internal class GeneralTests_Target
        {
            public GeneralTests_Target()
            {
            }

            public GeneralTests_Target(int a)
            {
            }
        }


        internal class GeneralTests_Target2
        {
            [GeneralTests_Trigger]
            public void Do()
            {
            }
        }

        internal interface IGeneralTests
        {
            string Fact(string data, int value, ref object testRef, out object testOut, ref int testRefValue, out int testOutValue);

            event EventHandler<EventArgs> TestEvent;

            int TestProperty { get; set; }
        }

        [Injection(typeof(GeneralTests_Aspect))]
        private class GeneralTests_Trigger : Attribute
        {
        }

        [Mixin(typeof(IGeneralTests))]
        [Mixin(typeof(INotifyPropertyChanged))]
        [Aspect(Scope.Global)]
        internal class GeneralTests_Aspect : IGeneralTests, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

            string IGeneralTests.Fact(string data, int value, ref object testRef, out object testOut, ref int testRefValue, out int testOutValue)
            {
                Checker.Passed = true;
                testOut = new object();
                testOutValue = 0;
                return data;
            }

            public event EventHandler<System.EventArgs> TestEvent
            {
                add { Checker.Passed = true; }
                remove { Checker.Passed = true; }
            }

            public int TestProperty
            {
                get { Checker.Passed = true; return 0; }
                set { Checker.Passed = true; }
            }
        }
    }
}