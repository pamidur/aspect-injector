using AspectInjector.Broker;
using System;
using System.ComponentModel;
using System.Linq;

namespace AspectInjector.SampleApps.NotifyPropertyChanged
{
    [AttributeUsage(AttributeTargets.Property)]
    [Injection(typeof(NotifyAspect))]
    public class Notify : Attribute
    {
        public string NotifyAlso { get; set; }
    }


    [Mixin(typeof(INotifyPropertyChanged))]
    [Aspect(Scope.PerInstance)]
    internal class NotifyAspect : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        [Advice(Kind.After, Targets = Target.Public | Target.Setter)]
        public void AfterSetter(
            [Argument(Source.Instance)] object source,
            [Argument(Source.Name)] string propName,
            [Argument(Source.Injections)] Attribute[] injections
            )
        {
            PropertyChanged(source, new PropertyChangedEventArgs(propName));

            foreach(var i in injections.OfType<Notify>().ToArray())
                PropertyChanged(source, new PropertyChangedEventArgs(i.NotifyAlso));
        }
    }
}