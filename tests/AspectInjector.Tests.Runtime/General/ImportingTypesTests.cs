using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;

namespace AspectInjector.Tests.General
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

    [IncutSpecification(typeof(NotifyPropertyChangedAspect))]
    internal class NotifyAttribute : Attribute
    {
        public string NotifyAlso { get; set; }
    }

    [Mixin(typeof(INotifyPropertyChanged))]
    internal class NotifyPropertyChangedAspect : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        [Advice(Advice.Type.After, Advice.Target.Setter)]
        public void AfterSetter(
            [AdviceArgument(AdviceArgument.Source.Instance)] object source,
            [AdviceArgument(AdviceArgument.Source.Name)] string propName,
            [AdviceArgument(AdviceArgument.Source.RoutableData)] Attribute[] data
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