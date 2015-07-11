using AspectInjector.Broker;
using System;
using System.ComponentModel;

namespace SampleApps.NotifyPropertyChanged.Aspects
{
    [CustomAspectDefinition(typeof(NotifyPropertyChangedAspect))]
    class NotifyAttribute : Attribute
    {
        public string NotifyAlso { get; set; }
    }


    [AdviceInterfaceProxy(typeof(INotifyPropertyChanged))]
    class NotifyPropertyChangedAspect : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        [Advice(InjectionPoints.After, InjectionTargets.Setter)]
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
