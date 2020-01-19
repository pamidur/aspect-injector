using Aspects.Freezable;
using System;
using Xunit;

namespace Aspests.Tests
{
    public class FreezableTests
    {
        [Freezable]
        class TestClass
        {
            public string Data { get; set; }
        }

        [Fact]
        public void Frozen_Object_Does_Not_Allow_Change_Property()
        {
            var target = new TestClass();
            target.Data = "test1";
            ((IFreezable)target).Freeze();

            Assert.Throws<InvalidOperationException>(() => target.Data = "test2");
        }
    }
}
