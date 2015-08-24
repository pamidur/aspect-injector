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

    [CustomAspectDefinition(typeof(NotifyPropertyChangedAspect))]
    class NotifyAttribute : Attribute
    {
        public string NotifyAlso { get; set; }
    }


    [AdviceInterfaceProxy(typeof(INotifyPropertyChanged))]
    class NotifyPropertyChangedAspect : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        [Advice(InjectionPoints.Before, InjectionTargets.Setter)]
        public void CheckIfUpdated(
            [AdviceArgument(AdviceArgumentSource.TargetValue)] object oldvalue,
            [AdviceArgument(AdviceArgumentSource.TargetArguments)] object[] newvalue,
            [AdviceArgument(AdviceArgumentSource.AbortFlag)] ref bool abort
            )
        {
            abort = oldvalue.Equals(newvalue[0]);
        }

        [Advice(InjectionPoints.After,InjectionTargets.Setter)]
        public void AfterSetter(
            [AdviceArgument(AdviceArgumentSource.Instance)] object source,
            [AdviceArgument(AdviceArgumentSource.TargetName)] string propName,
            [AdviceArgument(AdviceArgumentSource.RoutableData)] object data
            )
        {
            PropertyChanged(source, new PropertyChangedEventArgs(propName));

            var additionalPropName = (data as NotifyAttribute).NotifyAlso;

            if (!string.IsNullOrEmpty(additionalPropName))
            {
                PropertyChanged(source, new PropertyChangedEventArgs(additionalPropName));
            }
        }
    }
}
