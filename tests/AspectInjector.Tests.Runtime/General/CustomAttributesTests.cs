using AspectInjector.Broker;
using System;
using System.Linq;
using Xunit;

namespace AspectInjector.Tests.General
{
    public class InjectionDataTests
    {
        [Fact]
        public void Injection_Parameters_Are_Correct()
        {
            var a = new TestClass();

            Checker.Passed = false;
            a.Do();
            Assert.True(Checker.Passed);

            Checker.Passed = false;
            a.Do2();
            Assert.True(Checker.Passed);
        }

        private class TestClass
        {
            [TestInjection(true, byte.MaxValue, sbyte.MinValue, char.MaxValue, short.MinValue, ushort.MaxValue, int.MinValue, uint.MaxValue, float.MinValue, long.MinValue, ulong.MaxValue, double.MinValue)]
            public void Do() { }

            [TestInjection2(new ulong[] { ulong.MinValue, ulong.MaxValue }, 2, new object[] { new object[] { 0.5f, true }, 1, typeof(TestInjection2) }, new double[] { 12d, -0.777d }, new float[] { }, new bool[] { true }, typeof(TestClass), StringComparison.InvariantCulture)]
            public void Do2() { }
        }

        [Injection(typeof(TestAspect))]
        private class TestInjection2 : Attribute
        {
            public TestInjection2(object[] oa)
            {
            }

            public TestInjection2(ulong[] ul, object o, object[] oa, double[] da, float[] fa, bool[] ba, Type type, StringComparison e)
            {
                Ul = ul;
                O = o;
                Oa = oa;
                Da = da;
                Fa = fa;
                Ba = ba;
                Type = type;
                E = e;
            }

            public ulong[] Ul { get; }
            public object O { get; }
            public object[] Oa { get; }
            public double[] Da { get; }
            public float[] Fa { get; }
            public bool[] Ba { get; }
            public Type Type { get; }
            public StringComparison E { get; }
        }


        [Injection(typeof(TestAspect))]
        private class TestInjection : Attribute
        {
            public TestInjection(bool bo, byte b, sbyte sb, char c, short s, ushort us, int i, uint ui, float f, long l, ulong ul, double d)
            {
                Bo = bo;
                B = b;
                Sb = sb;
                C = c;
                S = s;
                Us = us;
                I = i;
                Ui = ui;
                F = f;
                L = l;
                Ul = ul;
                D = d;
            }

            public bool Bo { get; }
            public byte B { get; }
            public sbyte Sb { get; }
            public char C { get; }
            public short S { get; }
            public ushort Us { get; }
            public int I { get; }
            public uint Ui { get; }
            public float F { get; }
            public long L { get; }
            public ulong Ul { get; }
            public double D { get; }
        }

        [Aspect(Scope.Global)]
        public class TestAspect
        {
            [Advice(Kind.Before)]
            public void Before2([Argument(Source.Triggers)] Attribute[] data)
            {
                var a = data.OfType<TestInjection2>().FirstOrDefault();
                if (a == null) return;



                Checker.Passed = a.Ul[0]==ulong.MinValue && a.Ul[1] == ulong.MaxValue && (int)a.O == 2 && (float)((object[])a.Oa[0])[0] == 0.5f && (bool)((object[])a.Oa[0])[1] == true
                    && (int)a.Oa[1] == 1 && (Type)a.Oa[2] == typeof(TestInjection2) && a.Da[0] == 12d && a.Da[1] == -0.777d
                    && a.Fa.Length == 0 && a.Ba[0] == true && a.Type == typeof(TestClass) && a.E == StringComparison.InvariantCulture;
            }

            [Advice(Kind.Before)]
            public void Before([Argument(Source.Triggers)] Attribute[] data)
            {
                var a = data.OfType<TestInjection>().FirstOrDefault();
                if (a == null) return;

                Checker.Passed = a.Bo && a.B == byte.MaxValue && a.Sb == sbyte.MinValue && a.C == char.MaxValue &&
                    a.S == short.MinValue && a.Us == ushort.MaxValue && a.I == int.MinValue && a.Ui == uint.MaxValue &&
                    a.F == float.MinValue && a.L == long.MinValue && a.Ul == ulong.MaxValue && a.D == double.MinValue;
            }
        }
    }


    public class CustomAttributesTests
    {
        [Fact]
        public void General_CustomAttributes_PassRoutableValues()
        {
            Checker.Passed = false;

            var a = new CustomAttributesTests_Target();
            a.Do();

            Assert.True(Checker.Passed);

            var b = new CustomAttributesTestsAttribute("111") { Value = "olo" };
        }

        [Fact]
        public void General_CustomAttributes_Multiple()
        {
            Checker.Passed = false;

            var a = new CustomAttributesTests_MultipleTarget();
            a.Do123();

            Assert.True(Checker.Passed);
        }
    }

    [CustomAttributesTests("TestHeader", Value = "ololo", data = 43)]
    internal class CustomAttributesTests_Target
    {
        public void Do()
        {
        }
    }

    [CustomAttributesTests_Multiple1]
    internal class CustomAttributesTests_MultipleTarget
    {
        [CustomAttributesTests_Multiple2]
        public void Do123()
        {
        }
    }

    [Injection(typeof(CustomAttributesTests_Aspect))]
    internal class CustomAttributesTestsAttribute : Attribute
    {
        public string Header { get; private set; }

        public string Value { get; set; }

        public int data = 42;

        public CustomAttributesTestsAttribute(string header)
        {
            Header = header;
        }
    }

    [Aspect(Scope.Global)]
    public class CustomAttributesTests_Aspect
    {
        [Advice(Kind.After)]
        public void AfterMethod([Argument(Source.Triggers)] Attribute[] data)
        {
            var a = (data[0] as CustomAttributesTestsAttribute);

            Checker.Passed = a.Header == "TestHeader" && a.Value == "ololo" && a.data == 43;
        }
    }

    [Injection(typeof(CustomAttributesTests_MultipleAspect))]
    public class CustomAttributesTests_Multiple1Attribute : Attribute
    {
    }

    [Injection(typeof(CustomAttributesTests_MultipleAspect))]
    public class CustomAttributesTests_Multiple2Attribute : Attribute
    {
    }

    [Aspect(Scope.Global)]
    public class CustomAttributesTests_MultipleAspect
    {
        [Advice(Kind.After)]
        public void AfterMethod([Argument(Source.Triggers)] Attribute[] data)
        {
            Checker.Passed = data.Length == 2
                && data[0] is CustomAttributesTests_Multiple1Attribute
                && data[1] is CustomAttributesTests_Multiple2Attribute;
        }
    }
}