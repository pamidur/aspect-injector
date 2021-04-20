using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Aspects.StringLengthCheck;
using Xunit;

namespace Aspests.Tests
{
    public class StringLengthCheckTests
    {
        class TestClass
        {
            [StringLengthCheck(10, 1)]
            public string Name { get; set; }

            public string NonCheck { get; set; }
        }

        [Fact]
        public void Violation_Exception_Test()
        {
            Assert.Throws<ArgumentException>(() => new TestClass().Name = string.Empty);
        }

        [Fact]
        public void NonCheck_String_Test()
        {
            var t = new TestClass() { NonCheck = string.Empty };

            Assert.Empty(t.NonCheck);
        }
    }
}
