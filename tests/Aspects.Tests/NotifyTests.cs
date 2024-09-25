using Aspects.Notify;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit;

namespace Aspests.Tests
{
    public class NotifyTests
    {
        class TestClass
        {
            [Notify]
            [NotifyAlso(nameof(FullName))]
            public string FirstName { get; set; }

            [Notify]
            [NotifyAlso(nameof(FullName))]
            public string LastName { get; set; }

            public byte Age { get; set; }

            public string FullName => $"{FirstName} {LastName}";

        }

        [Fact]
        public void Notify_Aspect_Should_Raise_Events()
        {
            var target = new TestClass();

            var raised = new List<string>();

            (target as INotifyPropertyChanged).PropertyChanged += (s, e) =>
            {
                raised.Add(e.PropertyName);
            };

            target.FirstName = "Alex";
            Assert.Collection(raised,
                s => Assert.Equal(nameof(target.FirstName), s),
                s => Assert.Equal(nameof(target.FullName), s)
                );
            raised.Clear();

            target.Age = 92;
            Assert.Empty(raised);
        }
    }
}
