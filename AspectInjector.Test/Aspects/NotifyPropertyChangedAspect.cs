using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Test.Aspects
{
    [InjectInterfaceProxy(Interface = typeof(INotifyPropertyChanged))]
    public class NotifyPropertyChangedAspect : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        [PropertyInjection(Method = PropertyMethod.Set, Point = MethodPoint.Ending)]
        public void RaisePropertyChanged(
            [InjectArgument(Argument = InjectArgument.Instance)] object target,
            [InjectArgument(Argument = InjectArgument.MemberName)]string propertyName)
        {
            PropertyChanged(target, new PropertyChangedEventArgs(propertyName));
        }
    }
}