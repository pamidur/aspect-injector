using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;

namespace AspectInjector.Tests
{
    [TestClass]
    public class ImportingTypesTests
    {
    }

    [Notify]
    public class AppViewModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
    }

    [AspectDefinition(typeof(NotifyPropertyChangedAspect))]
    internal class NotifyAttribute : Attribute
    {
        public string NotifyAlso { get; set; }
    }

    [AdviceInterfaceProxy(typeof(INotifyPropertyChanged))]
    internal class NotifyPropertyChangedAspect : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        [Advice(InjectionPoints.After, InjectionTargets.Setter)]
        public void AfterSetter(
            [AdviceArgument(AdviceArgumentSource.Instance)] object source,
            [AdviceArgument(AdviceArgumentSource.Name)] string propName,
            [AdviceArgument(AdviceArgumentSource.RoutableData)] Attribute[] data
            )
        {
            PropertyChanged(source, new PropertyChangedEventArgs(propName));

            var additionalPropName = (data[0] as NotifyAttribute).NotifyAlso;

            if (!string.IsNullOrEmpty(additionalPropName))
            {
                PropertyChanged(source, new PropertyChangedEventArgs(additionalPropName));
            }
        }
    }
}