using AspectInjector.Broker;

namespace AspectInjector.SampleApps.NotifyPropertyChanged
{
    public class AppViewModel
    {
        [Inject(typeof(NotifyAspect))]
        public string FirstName { get; set; }

        [Inject(typeof(NotifyAspect))]
        public string LastName { get; set; }
    }
}