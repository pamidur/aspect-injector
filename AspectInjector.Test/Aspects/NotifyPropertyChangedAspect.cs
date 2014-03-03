using AspectInjector.Broker;
using System;
using System.ComponentModel;

namespace AspectInjector.Test.Aspects
{
    [AdviceInterfaceProxy(typeof(INotifyPropertyChanged))]
    public class NotifyPropertyChangedAspect : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        [Advice(Targets = InjectionTarget.Setter, Points = InjectionPoint.After)]
        public void RaisePropertyChanged(
            [AdviceArgument(Source = AdviceArgumentSource.Instance)] object targetInstance,
            [AdviceArgument(Source = AdviceArgumentSource.TargetName)] string propertyName)
        {
            Console.WriteLine("Raising PropertyChange event on {0} for property {1}", targetInstance, propertyName);
            PropertyChanged(targetInstance, new PropertyChangedEventArgs(propertyName));
        }
    }
}