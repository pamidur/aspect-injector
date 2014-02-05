using System.ComponentModel;

namespace AspectInjector.Test.Aspects
{
    //[InterfaceProxyInjection(typeof(INotifyPropertyChanged))]
    public class NotifyPropertyChangedAspect : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        [PropertyInjection(PropertyMethod.Set, MethodPoint.Ending)]
        public void RaisePropertyChanged(
            [ArgumentInjection(ArgumentValue.Instance)] object target,
            [ArgumentInjection(ArgumentValue.MemberName)] string propertyName)
        {
            PropertyChanged(target, new PropertyChangedEventArgs(propertyName));
        }
    }
}