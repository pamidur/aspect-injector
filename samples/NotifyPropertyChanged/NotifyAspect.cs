using AspectInjector.Broker;
using System.ComponentModel;

namespace AspectInjector.SampleApps.NotifyPropertyChanged
{
    [Mixin(typeof(INotifyPropertyChanged))]
    [Aspect(Aspect.Scope.PerInstance)]
    internal class NotifyAspect : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        [Advice(Advice.Type.After, Advice.Target.Setter)]
        public void AfterSetter(
            [Advice.Argument(Advice.Argument.Source.Instance)] object source,
            [Advice.Argument(Advice.Argument.Source.Name)] string propName
            )
        {
            PropertyChanged(source, new PropertyChangedEventArgs(propName));
        }
    }
}